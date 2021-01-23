using System;

namespace ENet.Managed.Compressors
{
    internal sealed class ENetSentinelCompressor : ENetCompressor
    {
        public override void BeginCompress(int limit)
        {
            throw new NotImplementedException();
        }

        public override void CompressChunk(ReadOnlySpan<byte> Buffer)
        {
            throw new NotImplementedException();
        }

        public override int Decompress(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer)
        {
            throw new NotImplementedException();
        }

        public override int EndCompress(Span<byte> buffer)
        {
            throw new NotImplementedException();
        }
    }
}
