using System;

namespace ENet.Managed
{
    [Flags]
    public enum ENetPacketFlags : uint
    {
        /// <summary>
        /// Indicates packet data must be sent reliable
        /// </summary>
        Reliable = (1 << 0),

        /// <summary>
        /// Indicates packet data must be sent unsequenced
        /// </summary>
        Unsequenced = (1 << 1),

        /// <summary>
        /// Indicates packet data must be sent reliable but unsqeuenced
        /// </summary>
        /// <remarks>
        /// This flag is combination of <see cref="Reliable"/> and <see cref="Unsequenced"/>
        /// </remarks>
        UnsequencedReliable = Reliable | Unsequenced,

        /// <summary>
        /// Specifies to enet_packet_create to avoid allocating new buffer and use the given buffer directly
        /// </summary>
        /// <remarks>
        /// Given buffer must have allocated using the same allocater that ENet is using
        /// </remarks>
        NoAllocate = (1 << 2),

        /// <summary>
        /// Indicates the packet data fragments must be sent unreliable
        /// </summary>
        UnreliableFragment = (1 << 3),
    }
}
