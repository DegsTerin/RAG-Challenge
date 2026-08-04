// Purpose: Materialises parser input into bounded memory without exposing paths or allowing a parser to read beyond the authorised byte limit.
using System.Buffers;

using RagChallenge.Application.Documents;

namespace RagChallenge.Infrastructure.Documents;

internal static class BoundedDocumentReader
{
    private const int BufferSize = 16 * 1024;

    internal static async Task<byte[]> ReadAsync(
        Stream content,
        long maximumByteLength,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanRead)
        {
            throw new ArgumentException("Document content must be readable.", nameof(content));
        }

        if (content.CanSeek && content.Length - content.Position > maximumByteLength)
        {
            throw new DocumentParseException(DocumentParseFailureKind.LimitExceeded);
        }

        await using var bufferStream = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

        try
        {
            while (true)
            {
                var read = await content
                    .ReadAsync(buffer.AsMemory(0, BufferSize), cancellationToken)
                    .ConfigureAwait(false);

                if (read == 0)
                {
                    break;
                }

                if (bufferStream.Length + read > maximumByteLength)
                {
                    throw new DocumentParseException(
                        DocumentParseFailureKind.LimitExceeded);
                }

                await bufferStream
                    .WriteAsync(buffer.AsMemory(0, read), cancellationToken)
                    .ConfigureAwait(false);
            }

            if (bufferStream.Length == 0)
            {
                throw new DocumentParseException(
                    DocumentParseFailureKind.MalformedContent);
            }

            return bufferStream.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }
}
