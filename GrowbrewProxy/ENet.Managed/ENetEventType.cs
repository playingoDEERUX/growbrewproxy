namespace ENet.Managed
{
    public enum ENetEventType
    {
        /// <summary>
        /// Indicates no event
        /// </summary>
        None,

        /// <summary>
        /// Indicates event is about connection of a peer
        /// </summary>
        Connect,

        /// <summary>
        /// Indicates event is about disconnection of a peer
        /// </summary>
        Disconnect,

        /// <summary>
        /// Indicates event is about receiving a packet from a peer
        /// </summary>
        Receive,
    }
}
