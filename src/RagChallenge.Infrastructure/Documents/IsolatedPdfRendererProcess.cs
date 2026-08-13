// Purpose: Runs one bounded PDF render in the existing Server.Api executable over private binary pipes; it applies containment before sending untrusted bytes and kills the complete worker tree on failure.
using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using RagChallenge.Application.Documents;
using RagChallenge.Application.Persistence;
using RagChallenge.Domain.CorpusCatalog;

namespace RagChallenge.Infrastructure.Documents;

public sealed class RendererWorkerLaunch
{
    public RendererWorkerLaunch(
        string executablePath,
        IEnumerable<string>? argumentPrefix,
        string effectiveRuntimeIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(effectiveRuntimeIdentifier);
        ExecutablePath = executablePath;
        ArgumentPrefix = (argumentPrefix ?? Array.Empty<string>()).ToArray();
        EffectiveRuntimeIdentifier = effectiveRuntimeIdentifier;
    }

    public string ExecutablePath { get; }

    public IReadOnlyList<string> ArgumentPrefix { get; }

    public string EffectiveRuntimeIdentifier { get; }
}

public sealed class IsolatedPdfRendererProcess : IPdfPageRenderer
{
    private readonly RendererWorkerLaunch launch;

    public IsolatedPdfRendererProcess(RendererWorkerLaunch launch)
    {
        this.launch = launch ?? throw new ArgumentNullException(nameof(launch));
    }

    public RendererDescriptor Describe(PdfRenderPolicy policy) =>
        PdfPagePngV1RendererIdentity.CreateDescriptor(
            policy,
            launch.EffectiveRuntimeIdentifier);

    public async Task<PdfRenderResult> RenderAsync(
        VerifiedContentObject source,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(policy);

        if (source.ByteLength > policy.MaximumSourceByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        using var process = StartWorker();
        using var containment = WindowsJobContainment.Attach(process, policy);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(policy.WorkerTimeout);
        var errorDrain = process.StandardError.BaseStream.CopyToAsync(
            Stream.Null,
            CancellationToken.None);

        try
        {
            await PdfRenderWorkerProtocol.WriteRequestAsync(
                process.StandardInput.BaseStream,
                source,
                policy,
                timeout.Token).ConfigureAwait(false);
            process.StandardInput.Close();
            var result = await PdfRenderWorkerProtocol.ReadResponseAsync(
                process.StandardOutput.BaseStream,
                policy,
                timeout.Token).ConfigureAwait(false);
            await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
            await errorDrain.ConfigureAwait(false);

            if (process.ExitCode != 0 || result.RendererDescriptor != Describe(policy))
            {
                throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            KillCompleteTree(process);
            await DrainAfterFailureAsync(errorDrain).ConfigureAwait(false);
            throw new PdfRenderException(
                cancellationToken.IsCancellationRequested
                    ? PdfRenderFailureKind.Cancelled
                    : PdfRenderFailureKind.TimedOut);
        }
        catch (PdfRenderException)
        {
            KillCompleteTree(process);
            await DrainAfterFailureAsync(errorDrain).ConfigureAwait(false);
            throw;
        }
        catch
        {
            KillCompleteTree(process);
            await DrainAfterFailureAsync(errorDrain).ConfigureAwait(false);
            throw new PdfRenderException(PdfRenderFailureKind.RendererFailed);
        }
    }

    private Process StartWorker()
    {
        var start = new ProcessStartInfo
        {
            FileName = launch.ExecutablePath,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        foreach (var argument in launch.ArgumentPrefix)
        {
            start.ArgumentList.Add(argument);
        }

        start.ArgumentList.Add(PdfRenderWorker.ModeArgument);
        start.Environment.Clear();
        start.Environment["DOTNET_EnableDiagnostics"] = "0";
        start.Environment["COMPlus_EnableDiagnostics"] = "0";
        var process = new Process { StartInfo = start };

        try
        {
            if (!process.Start())
            {
                throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
            }

            return process;
        }
        catch (PdfRenderException)
        {
            process.Dispose();
            throw;
        }
        catch
        {
            process.Dispose();
            throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }
    }

    private static void KillCompleteTree(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                process.WaitForExit();
            }
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task DrainAfterFailureAsync(Task errorDrain)
    {
        try
        {
            await errorDrain.ConfigureAwait(false);
        }
        catch (IOException)
        {
        }
    }
}

public static class PdfRenderWorker
{
    public const string ModeArgument = "--pdf-render-worker-v1";

    public static bool IsWorkerMode(string[] args) =>
        args.Length == 1 && string.Equals(args[0], ModeArgument, StringComparison.Ordinal);

    public static async Task<int> RunAsync(
        Stream input,
        Stream output,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await PdfRenderWorkerProtocol.ReadRequestHeaderAsync(
                input,
                cancellationToken).ConfigureAwait(false);
            WorkerResourceLimits.Apply(request.Policy);
            var source = await PdfRenderWorkerProtocol.ReadRequestBodyAsync(
                input,
                request,
                cancellationToken).ConfigureAwait(false);
            await PdfToImagePdfPageRenderer.RenderToAsync(
                source,
                request.Policy,
                RuntimeInformation.RuntimeIdentifier,
                (descriptor, pageCount, token) =>
                    PdfRenderWorkerProtocol.WriteSuccessHeaderAsync(
                        output,
                        descriptor,
                        pageCount,
                        token),
                (page, totalBytes, token) =>
                    PdfRenderWorkerProtocol.WritePageAsync(
                        output,
                        page,
                        totalBytes,
                        request.Policy,
                        token),
                cancellationToken).ConfigureAwait(false);
            return 0;
        }
        catch (PdfRenderException exception)
        {
            await PdfRenderWorkerProtocol.TryWriteFailureAsync(
                output,
                exception.FailureKind,
                cancellationToken).ConfigureAwait(false);
            return 2;
        }
        catch (OperationCanceledException)
        {
            await PdfRenderWorkerProtocol.TryWriteFailureAsync(
                output,
                PdfRenderFailureKind.Cancelled,
                CancellationToken.None).ConfigureAwait(false);
            return 3;
        }
        catch
        {
            await PdfRenderWorkerProtocol.TryWriteFailureAsync(
                output,
                PdfRenderFailureKind.RendererFailed,
                CancellationToken.None).ConfigureAwait(false);
            return 4;
        }
    }
}

internal static class PdfRenderWorkerProtocol
{
    private static readonly byte[] RequestMagic = Encoding.ASCII.GetBytes("RCPDFR1\0");
    private static readonly byte[] ResponseMagic = Encoding.ASCII.GetBytes("RCPDFS1\0");

    internal static async Task WriteRequestAsync(
        Stream output,
        VerifiedContentObject source,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken)
    {
        using var header = new MemoryStream();
        using (var writer = new BinaryWriter(header, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(RequestMagic);
            writer.Write(policy.MaximumSourceByteLength);
            writer.Write(policy.MaximumPageCount);
            writer.Write(policy.MaximumTotalPixels);
            writer.Write(policy.MaximumPageOutputByteLength);
            writer.Write(policy.MaximumTotalOutputByteLength);
            writer.Write(policy.MaximumWorkerMemoryBytes);
            writer.Write(policy.MaximumWorkerCpuTime.Ticks);
            writer.Write(policy.WorkerTimeout.Ticks);
            writer.Write(source.ByteLength);
            writer.Write(Convert.FromHexString(source.Sha256.Value));
        }

        await output.WriteAsync(header.GetBuffer().AsMemory(0, checked((int)header.Length)), cancellationToken)
            .ConfigureAwait(false);
        source.Content.Position = 0;
        var remaining = source.ByteLength;
        var buffer = new byte[81920];

        while (remaining > 0)
        {
            var read = await source.Content.ReadAsync(
                buffer.AsMemory(0, (int)Math.Min(buffer.Length, remaining)),
                cancellationToken).ConfigureAwait(false);

            if (read == 0)
            {
                throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
            }

            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            remaining -= read;
        }

        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<WorkerRequestHeader> ReadRequestHeaderAsync(
        Stream input,
        CancellationToken cancellationToken)
    {
        var fixedHeader = new byte[8 + 8 + 4 + (6 * 8) + 8 + 32];
        await input.ReadExactlyAsync(fixedHeader, cancellationToken).ConfigureAwait(false);
        var offset = 0;

        if (!fixedHeader.AsSpan(0, 8).SequenceEqual(RequestMagic))
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        offset += 8;
        var maximumSourceBytes = ReadInt64(fixedHeader, ref offset);
        var maximumPages = ReadInt32(fixedHeader, ref offset);
        var maximumPixels = ReadInt64(fixedHeader, ref offset);
        var maximumPageOutput = ReadInt64(fixedHeader, ref offset);
        var maximumTotalOutput = ReadInt64(fixedHeader, ref offset);
        var maximumMemory = ReadInt64(fixedHeader, ref offset);
        var maximumCpuTicks = ReadInt64(fixedHeader, ref offset);
        var timeoutTicks = ReadInt64(fixedHeader, ref offset);
        var sourceLength = ReadInt64(fixedHeader, ref offset);
        var expectedHash = fixedHeader.AsSpan(offset, 32).ToArray();
        var policy = new PdfRenderPolicy(
            maximumSourceBytes,
            maximumPages,
            maximumPixels,
            maximumPageOutput,
            maximumTotalOutput,
            maximumMemory,
            TimeSpan.FromTicks(maximumCpuTicks),
            TimeSpan.FromTicks(timeoutTicks));

        if (sourceLength is <= 0 || sourceLength > policy.MaximumSourceByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        return new WorkerRequestHeader(policy, sourceLength, expectedHash);
    }

    internal static async Task<byte[]> ReadRequestBodyAsync(
        Stream input,
        WorkerRequestHeader request,
        CancellationToken cancellationToken)
    {
        if (request.SourceLength > int.MaxValue)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        var source = new byte[checked((int)request.SourceLength)];
        await input.ReadExactlyAsync(source, cancellationToken).ConfigureAwait(false);

        if (!SHA256.HashData(source).AsSpan().SequenceEqual(request.ExpectedSha256))
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        return source;
    }

    internal static async Task WriteSuccessHeaderAsync(
        Stream output,
        RendererDescriptor rendererDescriptor,
        int sourcePageCount,
        CancellationToken cancellationToken)
    {
        using var header = new MemoryStream();
        using (var writer = new BinaryWriter(header, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(ResponseMagic);
            writer.Write(0);
            WriteBoundedString(writer, rendererDescriptor.Value);
            writer.Write(sourcePageCount);
        }

        await output.WriteAsync(header.GetBuffer().AsMemory(0, checked((int)header.Length)), cancellationToken)
            .ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static async Task WritePageAsync(
        Stream output,
        RenderedPdfPageCandidate page,
        long totalBytes,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken)
    {
        var bytes = page.PngBytes;

        if (bytes.Length > policy.MaximumPageOutputByteLength ||
            totalBytes > policy.MaximumTotalOutputByteLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.LimitExceeded);
        }

        var pageHeader = new byte[4 + 8 + 8 + 8];
        BinaryPrimitives.WriteInt32LittleEndian(pageHeader, page.PageNumber);
        BinaryPrimitives.WriteInt64LittleEndian(
            pageHeader.AsSpan(4),
            BitConverter.DoubleToInt64Bits(page.SourceWidthPoints));
        BinaryPrimitives.WriteInt64LittleEndian(
            pageHeader.AsSpan(12),
            BitConverter.DoubleToInt64Bits(page.SourceHeightPoints));
        BinaryPrimitives.WriteInt64LittleEndian(pageHeader.AsSpan(20), bytes.Length);
        await output.WriteAsync(pageHeader, cancellationToken).ConfigureAwait(false);
        await output.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal static async Task<PdfRenderResult> ReadResponseAsync(
        Stream input,
        PdfRenderPolicy policy,
        CancellationToken cancellationToken)
    {
        var prefix = new byte[12];
        await input.ReadExactlyAsync(prefix, cancellationToken).ConfigureAwait(false);

        if (!prefix.AsSpan(0, 8).SequenceEqual(ResponseMagic))
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        var status = BinaryPrimitives.ReadInt32LittleEndian(prefix.AsSpan(8));

        if (status != 0)
        {
            var failureValue = status - 1;
            throw new PdfRenderException(
                Enum.IsDefined((PdfRenderFailureKind)failureValue)
                    ? (PdfRenderFailureKind)failureValue
                    : PdfRenderFailureKind.RendererFailed);
        }

        var descriptor = await ReadBoundedStringAsync(input, 128, cancellationToken)
            .ConfigureAwait(false);
        var countBytes = new byte[4];
        await input.ReadExactlyAsync(countBytes, cancellationToken).ConfigureAwait(false);
        var pageCount = BinaryPrimitives.ReadInt32LittleEndian(countBytes);

        if (pageCount is <= 0 || pageCount > policy.MaximumPageCount)
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        var pages = new List<RenderedPdfPageCandidate>(pageCount);
        long totalBytes = 0;

        for (var index = 0; index < pageCount; index++)
        {
            var pageHeader = new byte[28];
            await input.ReadExactlyAsync(pageHeader, cancellationToken).ConfigureAwait(false);
            var pageNumber = BinaryPrimitives.ReadInt32LittleEndian(pageHeader);
            var widthPoints = BitConverter.Int64BitsToDouble(
                BinaryPrimitives.ReadInt64LittleEndian(pageHeader.AsSpan(4)));
            var heightPoints = BitConverter.Int64BitsToDouble(
                BinaryPrimitives.ReadInt64LittleEndian(pageHeader.AsSpan(12)));
            var byteLength = BinaryPrimitives.ReadInt64LittleEndian(pageHeader.AsSpan(20));
            totalBytes = checked(totalBytes + byteLength);

            if (byteLength is <= 0 or > int.MaxValue ||
                byteLength > policy.MaximumPageOutputByteLength ||
                totalBytes > policy.MaximumTotalOutputByteLength)
            {
                throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
            }

            var bytes = new byte[checked((int)byteLength)];
            await input.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
            pages.Add(new RenderedPdfPageCandidate(
                pageNumber,
                widthPoints,
                heightPoints,
                bytes));
        }

        return new PdfRenderResult(new RendererDescriptor(descriptor), pageCount, pages);
    }

    internal static async Task TryWriteFailureAsync(
        Stream output,
        PdfRenderFailureKind failureKind,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = new byte[12];
            ResponseMagic.CopyTo(response, 0);
            BinaryPrimitives.WriteInt32LittleEndian(
                response.AsSpan(8),
                checked((int)failureKind + 1));
            await output.WriteAsync(response, cancellationToken).ConfigureAwait(false);
            await output.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch
        {
        }
    }

    private static void WriteBoundedString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);

        if (bytes.Length is <= 0 or > 128)
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        writer.Write(bytes.Length);
        writer.Write(bytes);
    }

    private static async Task<string> ReadBoundedStringAsync(
        Stream input,
        int maximumLength,
        CancellationToken cancellationToken)
    {
        var lengthBytes = new byte[4];
        await input.ReadExactlyAsync(lengthBytes, cancellationToken).ConfigureAwait(false);
        var length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);

        if (length is <= 0 || length > maximumLength)
        {
            throw new PdfRenderException(PdfRenderFailureKind.ProtocolViolation);
        }

        var value = new byte[length];
        await input.ReadExactlyAsync(value, cancellationToken).ConfigureAwait(false);
        return Encoding.UTF8.GetString(value);
    }

    private static long ReadInt64(byte[] bytes, ref int offset)
    {
        var value = BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(offset));
        offset += 8;
        return value;
    }

    private static int ReadInt32(byte[] bytes, ref int offset)
    {
        var value = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(offset));
        offset += 4;
        return value;
    }

    internal sealed record WorkerRequestHeader(
        PdfRenderPolicy Policy,
        long SourceLength,
        byte[] ExpectedSha256);
}

internal static class WorkerResourceLimits
{
    private const int RlimitCpu = 0;
    private const int RlimitFileSize = 1;
    private const int RlimitCore = 4;
    private const int RlimitNoFile = 7;
    private const int RlimitAddressSpace = 9;
    private const int PrSetDumpable = 4;

    internal static void Apply(PdfRenderPolicy policy)
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        var cpuSeconds = checked((ulong)Math.Ceiling(policy.MaximumWorkerCpuTime.TotalSeconds));
        SetLimit(RlimitCpu, cpuSeconds);
        SetLimit(RlimitAddressSpace, checked((ulong)policy.MaximumWorkerMemoryBytes));
        SetLimit(RlimitFileSize, checked((ulong)policy.MaximumTotalOutputByteLength));
        SetLimit(RlimitCore, 0);
        SetLimit(RlimitNoFile, 64);

        if (Prctl(PrSetDumpable, 0, 0, 0, 0) != 0)
        {
            throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }
    }

    private static void SetLimit(int resource, ulong value)
    {
        var limit = new RLimit { Current = value, Maximum = value };

        if (SetRLimit(resource, ref limit) != 0)
        {
            throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }
    }

    [DllImport("libc", EntryPoint = "setrlimit", SetLastError = true)]
    private static extern int SetRLimit(int resource, ref RLimit limit);

    [DllImport("libc", EntryPoint = "prctl", SetLastError = true)]
    private static extern int Prctl(
        int option,
        ulong argument2,
        ulong argument3,
        ulong argument4,
        ulong argument5);

    [StructLayout(LayoutKind.Sequential)]
    private struct RLimit
    {
        internal ulong Current;
        internal ulong Maximum;
    }
}

internal sealed class WindowsJobContainment : IDisposable
{
    private const uint JobObjectLimitProcessTime = 0x00000002;
    private const uint JobObjectLimitActiveProcess = 0x00000008;
    private const uint JobObjectLimitProcessMemory = 0x00000100;
    private const uint JobObjectLimitKillOnJobClose = 0x00002000;
    private const int ExtendedLimitInformationClass = 9;

    private readonly IntPtr handle;

    private WindowsJobContainment(IntPtr handle)
    {
        this.handle = handle;
    }

    internal static IDisposable Attach(Process process, PdfRenderPolicy policy)
    {
        if (!OperatingSystem.IsWindows())
        {
            return NoopDisposable.Instance;
        }

        var handle = CreateJobObject(IntPtr.Zero, null);

        if (handle == IntPtr.Zero)
        {
            throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
        }

        var containment = new WindowsJobContainment(handle);

        try
        {
            var information = new JobObjectExtendedLimitInformation
            {
                BasicLimitInformation = new JobObjectBasicLimitInformation
                {
                    PerProcessUserTimeLimit = policy.MaximumWorkerCpuTime.Ticks,
                    LimitFlags = JobObjectLimitProcessTime |
                        JobObjectLimitActiveProcess |
                        JobObjectLimitProcessMemory |
                        JobObjectLimitKillOnJobClose,
                    ActiveProcessLimit = 1,
                },
                ProcessMemoryLimit = new UIntPtr(
                    checked((ulong)policy.MaximumWorkerMemoryBytes)),
            };
            var size = Marshal.SizeOf<JobObjectExtendedLimitInformation>();

            if (!SetInformationJobObject(
                    handle,
                    ExtendedLimitInformationClass,
                    ref information,
                    (uint)size) ||
                !AssignProcessToJobObject(handle, process.Handle))
            {
                throw new PdfRenderException(PdfRenderFailureKind.RendererUnavailable);
            }

            return containment;
        }
        catch
        {
            containment.Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        if (handle != IntPtr.Zero)
        {
            CloseHandle(handle);
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr jobAttributes, string? name);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetInformationJobObject(
        IntPtr job,
        int informationClass,
        ref JobObjectExtendedLimitInformation information,
        uint informationLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);

    [StructLayout(LayoutKind.Sequential)]
    private struct IoCounters
    {
        internal ulong ReadOperationCount;
        internal ulong WriteOperationCount;
        internal ulong OtherOperationCount;
        internal ulong ReadTransferCount;
        internal ulong WriteTransferCount;
        internal ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectBasicLimitInformation
    {
        internal long PerProcessUserTimeLimit;
        internal long PerJobUserTimeLimit;
        internal uint LimitFlags;
        internal UIntPtr MinimumWorkingSetSize;
        internal UIntPtr MaximumWorkingSetSize;
        internal uint ActiveProcessLimit;
        internal UIntPtr Affinity;
        internal uint PriorityClass;
        internal uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct JobObjectExtendedLimitInformation
    {
        internal JobObjectBasicLimitInformation BasicLimitInformation;
        internal IoCounters IoInfo;
        internal UIntPtr ProcessMemoryLimit;
        internal UIntPtr JobMemoryLimit;
        internal UIntPtr PeakProcessMemoryUsed;
        internal UIntPtr PeakJobMemoryUsed;
    }

    private sealed class NoopDisposable : IDisposable
    {
        internal static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
