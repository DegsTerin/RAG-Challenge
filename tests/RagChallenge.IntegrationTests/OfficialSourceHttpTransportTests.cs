// Purpose: Proves the product official-source transport with fake DNS, HTTP, socket and TLS collaborators only, without opening a real connection or resolving a real host.
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

using RagChallenge.Application.Documents;
using RagChallenge.Domain.CorpusCatalog;
using RagChallenge.Infrastructure.Documents;

namespace RagChallenge.IntegrationTests;

public sealed class OfficialSourceHttpTransportTests
{
    private static readonly Uri SourceUri = new(
        "https://www.postgresql.org/files/documentation/pdf/18/postgresql-18-A4.pdf",
        UriKind.Absolute);

    [Fact]
    public async Task FakeHttpExchangePreservesExactAuthorityAndBounds()
    {
        var bytes = "%PDF-1.4\nsynthetic-only\n%%EOF"u8.ToArray();
        var handler = new RecordingHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(bytes),
            };
            response.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue(
                "\"postgresql-18-a4\"");
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            response.Content.Headers.LastModified =
                new DateTimeOffset(2026, 8, 12, 12, 0, 0, TimeSpan.Zero);
            return response;
        });
        var factory = new RecordingHandlerFactory(handler);
        var transport = new OfficialSourceHttpTransport(factory);
        var result = await transport.FetchAsync(
            Registration(),
            new OfficialFetchPolicy(
                MaximumByteLength: 1024,
                IfNoneMatch: "\"prior\"",
                IfModifiedSince: new DateTimeOffset(
                    2026,
                    8,
                    11,
                    12,
                    0,
                    0,
                    TimeSpan.Zero)));

        Assert.Equal(OfficialFetchStatus.Changed, result.Status);
        Assert.Equal(bytes, result.Content);
        Assert.Equal("application/pdf", result.MediaType);
        Assert.Equal("\"postgresql-18-a4\"", result.ETag);
        Assert.Equal(SourceUri, factory.ApprovedUri);
        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(SourceUri, request.RequestUri);
        Assert.Equal(SourceUri.IdnHost, request.Headers.Host);
        Assert.Equal(HttpVersion.Version11, request.Version);
        Assert.Equal(HttpVersionPolicy.RequestVersionExact, request.VersionPolicy);
        Assert.Null(request.Headers.Authorization);
        Assert.Equal("\"prior\"", Assert.Single(request.Headers.IfNoneMatch).ToString());
    }

    [Fact]
    public async Task FakeHttpExchangeRejectsRedirectAndOversizedContentBeforePromotion()
    {
        var redirectHandler = new RecordingHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("https://example.com/redirected", UriKind.Absolute);
            response.Content = new ByteArrayContent([]);
            return response;
        });
        var redirectTransport = new OfficialSourceHttpTransport(
            new RecordingHandlerFactory(redirectHandler));
        await Assert.ThrowsAsync<IOException>(() => redirectTransport.FetchAsync(
            Registration(),
            new OfficialFetchPolicy(1024, null, null)));

        var oversizedHandler = new RecordingHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(new byte[17]),
            };
            response.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
            return response;
        });
        var oversizedTransport = new OfficialSourceHttpTransport(
            new RecordingHandlerFactory(oversizedHandler));
        await Assert.ThrowsAsync<IOException>(() => oversizedTransport.FetchAsync(
            Registration(),
            new OfficialFetchPolicy(16, null, null)));

        Assert.Single(redirectHandler.Requests);
        Assert.Single(oversizedHandler.Requests);
    }

    [Fact]
    public async Task FakeDnsAnswerIsAtomicAndRejectsAnyForbiddenAddress()
    {
        var mixedDns = new FakeDnsResolver(
            IPAddress.Parse("8.8.8.8"),
            IPAddress.Parse("10.0.0.7"));
        var mixedResolver = new OfficialSourceEndpointResolver(mixedDns);

        await Assert.ThrowsAsync<IOException>(() =>
            mixedResolver.ResolveAsync(SourceUri, CancellationToken.None));
        Assert.Equal(1, mixedDns.CallCount);

        var approvedDns = new FakeDnsResolver(
            IPAddress.Parse("8.8.8.8"),
            IPAddress.Parse("2606:4700:4700::1111"));
        var approvedResolver = new OfficialSourceEndpointResolver(approvedDns);
        var endpoints = await approvedResolver.ResolveAsync(
            SourceUri,
            CancellationToken.None);

        Assert.Equal(1, approvedDns.CallCount);
        Assert.Equal(2, endpoints.Count);
        Assert.All(endpoints, endpoint => Assert.Equal(443, endpoint.Port));
        Assert.Contains(endpoints, endpoint => endpoint.Address.Equals(IPAddress.Parse("8.8.8.8")));
        Assert.Contains(
            endpoints,
            endpoint => endpoint.Address.Equals(IPAddress.Parse("2606:4700:4700::1111")));
    }

    [Fact]
    public void FakeTlsCollaboratorObservesLocalOnlyPinnedHandlerPolicy()
    {
        var dns = new FakeDnsResolver(IPAddress.Parse("8.8.8.8"));
        var connector = new RejectingSocketConnector();
        var tls = new RecordingTlsValidator();
        var factory = new PinnedOfficialSourceHttpHandlerFactory(dns, connector, tls);
        using var handler = Assert.IsType<SocketsHttpHandler>(factory.Create(SourceUri));

        Assert.False(handler.AllowAutoRedirect);
        Assert.False(handler.UseProxy);
        Assert.False(handler.UseCookies);
        Assert.Null(handler.Credentials);
        Assert.False(handler.PreAuthenticate);
        Assert.Equal(1, handler.MaxConnectionsPerServer);
        Assert.Equal(TimeSpan.Zero, handler.PooledConnectionLifetime);
        Assert.Equal(
            X509RevocationMode.NoCheck,
            handler.SslOptions.CertificateRevocationCheckMode);
        Assert.True(handler.SslOptions.CertificateChainPolicy!.DisableCertificateDownloads);
        Assert.Equal(
            X509RevocationMode.NoCheck,
            handler.SslOptions.CertificateChainPolicy.RevocationMode);
        Assert.True(handler.SslOptions.RemoteCertificateValidationCallback!(
            new object(),
            null,
            null,
            SslPolicyErrors.None));
        Assert.Equal(1, tls.CallCount);
        Assert.Equal(0, dns.CallCount);
        Assert.Equal(0, connector.CallCount);
    }

    [Theory]
    [InlineData("127.0.0.1")]
    [InlineData("169.254.169.254")]
    [InlineData("192.0.2.10")]
    [InlineData("198.51.100.10")]
    [InlineData("203.0.113.10")]
    [InlineData("::1")]
    [InlineData("fc00::1")]
    [InlineData("fe80::1")]
    [InlineData("2001:db8::1")]
    public void AddressPolicyRejectsNonPublicAndDocumentationRanges(string value) =>
        Assert.False(OfficialSourceNetworkAddressPolicy.IsAllowed(IPAddress.Parse(value)));

    private static OfficialSourceRegistration Registration() =>
        new(
            new OfficialSourceRegistrationId("postgresql-18-reference-a4-official"),
            new SourceRegistrationRevision(1),
            new DatabaseProductId("postgresql-18"),
            new DocumentId("postgresql-18-reference-a4"),
            new SourceAdapterId("postgresql-official-pdf-v1"),
            SourceUri.AbsoluteUri,
            CatalogueItemStatus.Candidate);

    private sealed class RecordingHandlerFactory(HttpMessageHandler handler)
        : IOfficialSourceHttpHandlerFactory
    {
        internal Uri? ApprovedUri { get; private set; }

        public HttpMessageHandler Create(Uri approvedUri)
        {
            ApprovedUri = approvedUri;
            return handler;
        }
    }

    private sealed class RecordingHttpMessageHandler(
        Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        : HttpMessageHandler
    {
        internal List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(responseFactory(request));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // The fake retains captured request metadata for assertions after disposal.
        }
    }

    private sealed class FakeDnsResolver(params IPAddress[] addresses)
        : IOfficialSourceDnsResolver
    {
        internal int CallCount { get; private set; }

        public Task<IPAddress[]> ResolveAsync(
            string asciiHost,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Assert.Equal(SourceUri.IdnHost, asciiHost);
            return Task.FromResult(addresses.ToArray());
        }
    }

    private sealed class RejectingSocketConnector : IOfficialSourceSocketConnector
    {
        internal int CallCount { get; private set; }

        public ValueTask<Stream> ConnectAsync(
            IPEndPoint endpoint,
            CancellationToken cancellationToken)
        {
            CallCount++;
            throw new InvalidOperationException("A fake test must not open a socket.");
        }
    }

    private sealed class RecordingTlsValidator : IOfficialSourceTlsValidator
    {
        internal int CallCount { get; private set; }

        public bool Validate(
            X509Certificate? certificate,
            X509Chain? chain,
            SslPolicyErrors policyErrors)
        {
            CallCount++;
            Assert.Equal(SslPolicyErrors.None, policyErrors);
            return true;
        }
    }
}
