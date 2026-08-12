// Purpose: Implements the ADR-0006 official-source HTTPS transport with exact authority, one DNS decision per physical connection, explicit IP pinning, local TLS validation, and bounded fail-closed HTTP handling.
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Documents;

public sealed class OfficialSourceHttpTransport : IOfficialSourceTransport
{
    internal static readonly TimeSpan ResponseHeadersTimeout = TimeSpan.FromSeconds(30);
    internal static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(120);

    private readonly IOfficialSourceHttpHandlerFactory handlerFactory;

    public OfficialSourceHttpTransport()
        : this(new PinnedOfficialSourceHttpHandlerFactory())
    {
    }

    internal OfficialSourceHttpTransport(IOfficialSourceHttpHandlerFactory handlerFactory)
    {
        this.handlerFactory = handlerFactory ??
            throw new ArgumentNullException(nameof(handlerFactory));
    }

    public async Task<OfficialFetchResult> FetchAsync(
        OfficialSourceRegistration registration,
        OfficialFetchPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(policy);

        if (policy.MaximumByteLength <= 0 ||
            policy.IfNoneMatch?.Length > 512 ||
            policy.IfNoneMatch?.Any(char.IsControl) == true ||
            policy.IfModifiedSince is { } modifiedSince &&
            modifiedSince.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The official-source fetch policy is invalid.",
                nameof(policy));
        }

        var uri = OfficialSourceEndpointResolver.ParseExactAuthority(
            registration.CanonicalHttpsUrl);
        using var handler = handlerFactory.Create(uri);
        using var client = new HttpClient(handler, disposeHandler: false)
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };
        using var request = new HttpRequestMessage(HttpMethod.Get, uri)
        {
            Version = HttpVersion.Version11,
            VersionPolicy = HttpVersionPolicy.RequestVersionExact,
        };
        request.Headers.Host = uri.IdnHost;

        if (policy.IfNoneMatch is not null)
        {
            if (!EntityTagHeaderValue.TryParse(policy.IfNoneMatch, out var entityTag))
            {
                throw new ArgumentException(
                    "The official-source ETag validator is invalid.",
                    nameof(policy));
            }

            request.Headers.IfNoneMatch.Add(entityTag);
        }

        if (policy.IfModifiedSince is not null)
        {
            request.Headers.IfModifiedSince = policy.IfModifiedSince;
        }

        using var operationBudget = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken);
        operationBudget.CancelAfter(OperationTimeout);
        using var headerBudget = CancellationTokenSource.CreateLinkedTokenSource(
            operationBudget.Token);
        headerBudget.CancelAfter(ResponseHeadersTimeout);
        HttpResponseMessage response;

        try
        {
            response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                headerBudget.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IOException("The official-source response headers exceeded their time budget.");
        }
        catch (HttpRequestException exception)
        {
            throw new IOException("The official-source transport was unavailable.", exception);
        }

        using (response)
        {
            if ((response.RequestMessage?.RequestUri is not null &&
                    !Uri.Equals(response.RequestMessage.RequestUri, uri)) ||
                response.Headers.Location is not null ||
                response.Content.Headers.ContentEncoding.Count != 0 ||
                response.Content.Headers.ContentLength > policy.MaximumByteLength)
            {
                throw new IOException("The official-source response violated its exact HTTP policy.");
            }

            byte[] content;

            try
            {
                content = await ReadBoundedAsync(
                    response.Content,
                    policy.MaximumByteLength,
                    operationBudget.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new IOException("The official-source operation exceeded its time budget.");
            }
            catch (HttpRequestException exception)
            {
                throw new IOException("The official-source response could not be read.", exception);
            }

            var status = response.StatusCode switch
            {
                HttpStatusCode.OK => OfficialFetchStatus.Changed,
                HttpStatusCode.NotModified => OfficialFetchStatus.NotModified,
                HttpStatusCode.NotFound or HttpStatusCode.Gone =>
                    OfficialFetchStatus.Withdrawn,
                _ => throw new IOException(
                    "The official-source status code is not accepted by policy."),
            };

            if ((status == OfficialFetchStatus.Changed) != (content.Length > 0))
            {
                throw new IOException(
                    "The official-source response body does not match its status.");
            }

            var mediaType = response.Content.Headers.ContentType?.MediaType;

            if (status == OfficialFetchStatus.Changed &&
                string.IsNullOrWhiteSpace(mediaType))
            {
                throw new IOException("The official-source media type is missing.");
            }

            var etag = response.Headers.ETag?.ToString();

            if (etag?.Length > 512 || etag?.Any(char.IsControl) == true)
            {
                throw new IOException("The official-source ETag is invalid.");
            }

            return new OfficialFetchResult(
                status,
                (int)response.StatusCode,
                status == OfficialFetchStatus.Changed ? content : null,
                status == OfficialFetchStatus.Changed ? mediaType : null,
                etag,
                response.Content.Headers.LastModified?.ToUniversalTime());
        }
    }

    private static async Task<byte[]> ReadBoundedAsync(
        HttpContent content,
        long maximumByteLength,
        CancellationToken cancellationToken)
    {
        await using var source = await content.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        using var destination = new MemoryStream();
        var buffer = new byte[16 * 1024];

        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                return destination.ToArray();
            }

            if (destination.Length + read > maximumByteLength)
            {
                throw new IOException("The official-source response exceeded its byte limit.");
            }

            destination.Write(buffer, 0, read);
        }
    }
}

internal interface IOfficialSourceHttpHandlerFactory
{
    HttpMessageHandler Create(Uri approvedUri);
}

internal interface IOfficialSourceDnsResolver
{
    Task<IPAddress[]> ResolveAsync(
        string asciiHost,
        CancellationToken cancellationToken);
}

internal interface IOfficialSourceSocketConnector
{
    ValueTask<Stream> ConnectAsync(
        IPEndPoint endpoint,
        CancellationToken cancellationToken);
}

internal interface IOfficialSourceTlsValidator
{
    bool Validate(
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors policyErrors);
}

internal sealed class PinnedOfficialSourceHttpHandlerFactory(
    IOfficialSourceDnsResolver? dnsResolver = null,
    IOfficialSourceSocketConnector? socketConnector = null,
    IOfficialSourceTlsValidator? tlsValidator = null)
    : IOfficialSourceHttpHandlerFactory
{
    internal static readonly TimeSpan DnsAndConnectTimeout = TimeSpan.FromSeconds(10);

    private readonly OfficialSourceEndpointResolver endpointResolver =
        new(dnsResolver ?? new SystemOfficialSourceDnsResolver());
    private readonly IOfficialSourceSocketConnector socketConnector =
        socketConnector ?? new SystemOfficialSourceSocketConnector();
    private readonly IOfficialSourceTlsValidator tlsValidator =
        tlsValidator ?? new LocalOfficialSourceTlsValidator();

    public HttpMessageHandler Create(Uri approvedUri)
    {
        ArgumentNullException.ThrowIfNull(approvedUri);
        var exactUri = OfficialSourceEndpointResolver.ParseExactAuthority(
            approvedUri.AbsoluteUri);
        var chainPolicy = new X509ChainPolicy
        {
            DisableCertificateDownloads = true,
            RevocationMode = X509RevocationMode.NoCheck,
            RevocationFlag = X509RevocationFlag.ExcludeRoot,
            VerificationFlags = X509VerificationFlags.NoFlag,
            TrustMode = X509ChainTrustMode.System,
        };
        chainPolicy.ApplicationPolicy.Add(new Oid("1.3.6.1.5.5.7.3.1"));
        return new SocketsHttpHandler
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            UseCookies = false,
            Credentials = null,
            PreAuthenticate = false,
            AutomaticDecompression = DecompressionMethods.None,
            ConnectTimeout = DnsAndConnectTimeout,
            MaxConnectionsPerServer = 1,
            PooledConnectionLifetime = TimeSpan.Zero,
            EnableMultipleHttp2Connections = false,
            SslOptions = new SslClientAuthenticationOptions
            {
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                CertificateChainPolicy = chainPolicy,
                RemoteCertificateValidationCallback =
                    (_, certificate, chain, errors) =>
                        tlsValidator.Validate(certificate, chain, errors),
            },
            ConnectCallback = async (context, cancellationToken) =>
            {
                if (!string.Equals(
                        context.DnsEndPoint.Host,
                        exactUri.IdnHost,
                        StringComparison.OrdinalIgnoreCase) ||
                    context.DnsEndPoint.Port != 443)
                {
                    throw new IOException(
                        "The HTTP stack attempted an authority outside the approved source.");
                }

                using var budget = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);
                budget.CancelAfter(DnsAndConnectTimeout);
                var endpoints = await endpointResolver.ResolveAsync(
                    exactUri,
                    budget.Token).ConfigureAwait(false);
                Exception? lastFailure = null;

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        return await socketConnector.ConnectAsync(endpoint, budget.Token)
                            .ConfigureAwait(false);
                    }
                    catch (Exception exception) when (
                        exception is IOException or SocketException)
                    {
                        lastFailure = exception;
                    }
                }

                throw new IOException(
                    "No approved official-source endpoint accepted the connection.",
                    lastFailure);
            },
        };
    }
}

internal sealed class OfficialSourceEndpointResolver(IOfficialSourceDnsResolver dnsResolver)
{
    private readonly IOfficialSourceDnsResolver dnsResolver =
        dnsResolver ?? throw new ArgumentNullException(nameof(dnsResolver));

    internal async Task<IReadOnlyList<IPEndPoint>> ResolveAsync(
        Uri approvedUri,
        CancellationToken cancellationToken)
    {
        var exactUri = ParseExactAuthority(approvedUri.AbsoluteUri);
        var addresses = await dnsResolver.ResolveAsync(
            exactUri.IdnHost,
            cancellationToken).ConfigureAwait(false);

        if (addresses.Length == 0 ||
            addresses.Distinct().Count() != addresses.Length ||
            addresses.Any(address => !OfficialSourceNetworkAddressPolicy.IsAllowed(address)))
        {
            throw new IOException(
                "The official-source DNS answer was empty, duplicated, mixed, or prohibited.");
        }

        return addresses
            .OrderBy(address => address.AddressFamily)
            .ThenBy(address => Convert.ToHexString(address.GetAddressBytes()), StringComparer.Ordinal)
            .Select(address => new IPEndPoint(address, 443))
            .ToArray();
    }

    internal static Uri ParseExactAuthority(string canonicalHttpsUrl)
    {
        if (!Uri.TryCreate(canonicalHttpsUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal) ||
            uri.HostNameType != UriHostNameType.Dns ||
            !uri.IsDefaultPort ||
            uri.Port != 443 ||
            !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new ArgumentException(
                "An official-source transport requires one exact default-port HTTPS DNS authority.",
                nameof(canonicalHttpsUrl));
        }

        var asciiUri = new UriBuilder(uri)
        {
            Host = uri.IdnHost,
            Port = -1,
        }.Uri;

        if (!string.Equals(
                asciiUri.AbsoluteUri,
                canonicalHttpsUrl,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The official-source URI must already use its canonical IDNA ASCII authority.",
                nameof(canonicalHttpsUrl));
        }

        return asciiUri;
    }
}

internal static class OfficialSourceNetworkAddressPolicy
{
    internal static bool IsAllowed(IPAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) || address.Equals(IPAddress.Any) ||
            address.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        var bytes = address.GetAddressBytes();

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            return !HasPrefix(bytes, [0], 8) &&
                !HasPrefix(bytes, [10], 8) &&
                !HasPrefix(bytes, [100, 64], 10) &&
                !HasPrefix(bytes, [127], 8) &&
                !HasPrefix(bytes, [169, 254], 16) &&
                !HasPrefix(bytes, [172, 16], 12) &&
                !HasPrefix(bytes, [192, 0, 0], 24) &&
                !HasPrefix(bytes, [192, 0, 2], 24) &&
                !HasPrefix(bytes, [192, 168], 16) &&
                !HasPrefix(bytes, [198, 18], 15) &&
                !HasPrefix(bytes, [198, 51, 100], 24) &&
                !HasPrefix(bytes, [203, 0, 113], 24) &&
                !HasPrefix(bytes, [224], 4) &&
                !HasPrefix(bytes, [240], 4);
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return false;
        }

        return !address.IsIPv6LinkLocal &&
            !address.IsIPv6Multicast &&
            !address.IsIPv6SiteLocal &&
            !HasPrefix(bytes, [0xfc], 7) &&
            !HasPrefix(bytes, [0x20, 0x01, 0x0d, 0xb8], 32) &&
            !HasPrefix(bytes, [0x20, 0x01, 0x00, 0x00], 32) &&
            !HasPrefix(bytes, [0x20, 0x02], 16);
    }

    private static bool HasPrefix(byte[] address, byte[] prefix, int prefixBits)
    {
        var fullBytes = prefixBits / 8;
        var remainingBits = prefixBits % 8;

        for (var index = 0; index < fullBytes; index++)
        {
            if (address[index] != prefix[index])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(0xff << (8 - remainingBits));
        return (address[fullBytes] & mask) == (prefix[fullBytes] & mask);
    }
}

internal sealed class SystemOfficialSourceDnsResolver : IOfficialSourceDnsResolver
{
    public Task<IPAddress[]> ResolveAsync(
        string asciiHost,
        CancellationToken cancellationToken) =>
        Dns.GetHostAddressesAsync(asciiHost, cancellationToken);
}

internal sealed class SystemOfficialSourceSocketConnector : IOfficialSourceSocketConnector
{
    public async ValueTask<Stream> ConnectAsync(
        IPEndPoint endpoint,
        CancellationToken cancellationToken)
    {
        var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true,
        };

        try
        {
            await socket.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
    }
}

internal sealed class LocalOfficialSourceTlsValidator : IOfficialSourceTlsValidator
{
    private const string ServerAuthenticationOid = "1.3.6.1.5.5.7.3.1";

    public bool Validate(
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors policyErrors)
    {
        if (certificate is null || chain is null || policyErrors != SslPolicyErrors.None ||
            chain.ChainStatus.Any(status => status.Status != X509ChainStatusFlags.NoError))
        {
            return false;
        }

        var certificate2 = certificate as X509Certificate2 ??
            new X509Certificate2(certificate);
        var ownsCertificate = !ReferenceEquals(certificate2, certificate);
        var now = DateTimeOffset.UtcNow;

        try
        {
            var hasServerAuthentication = certificate2.Extensions
                .OfType<X509EnhancedKeyUsageExtension>()
                .SelectMany(extension => extension.EnhancedKeyUsages.Cast<Oid>())
                .Any(oid => string.Equals(
                    oid.Value,
                    ServerAuthenticationOid,
                    StringComparison.Ordinal));
            return now >= certificate2.NotBefore.ToUniversalTime() &&
                now <= certificate2.NotAfter.ToUniversalTime() &&
                hasServerAuthentication;
        }
        finally
        {
            if (ownsCertificate)
            {
                certificate2.Dispose();
            }
        }
    }
}
