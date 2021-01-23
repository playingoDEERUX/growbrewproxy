using System;

namespace ENet.Managed.Checksums
{
    internal sealed class ENetSentinelChecksum : ENetChecksum
    {
        public override void Begin()
        {
            throw new NotImplementedException();
        }

        public override uint End()
        {
            throw new NotImplementedException();
        }

        public override void Sum(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }
    }
}
