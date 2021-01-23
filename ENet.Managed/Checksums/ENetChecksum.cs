using System;

using ENet.Managed.Common;

namespace ENet.Managed.Checksums
{
    /// <summary>
    /// An abstract class which calculates checksums for given buffers by ENet library
    /// </summary>
    /// <remarks>
    /// Each instance of this class shouldn't be shared between multiple ENet Hosts
    /// </remarks>
    public unsafe abstract class ENetChecksum : DisposableBase
    {
        public abstract void Begin();
        public abstract void Sum(ReadOnlySpan<byte> buffer);
        public abstract uint End();
    }
}
