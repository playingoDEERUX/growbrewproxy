using System;

using ENet.Managed.Common;

namespace ENet.Managed.Compressors
{
    /// <summary>
    /// An abstract class which is de\compresses given buffers by ENet library
    /// </summary>
    /// <remarks>
    /// Each instance of this class shouldn't be shared between multiple ENet Hosts
    /// </remarks>
    public unsafe abstract class ENetCompressor : DisposableBase
    {
        public abstract void BeginCompress(int limit);
        public abstract void CompressChunk(ReadOnlySpan<byte> Buffer);
        public abstract int EndCompress(Span<byte> buffer);
        public abstract int Decompress(ReadOnlySpan<byte> inBuffer, Span<byte> outBuffer);
    }
}
