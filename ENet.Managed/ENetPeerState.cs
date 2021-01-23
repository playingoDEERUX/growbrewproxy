namespace ENet.Managed
{
    /// <summary>
    /// Peer state.
    /// </summary>
    public enum ENetPeerState
    {
        /// <summary>
        /// Indicates peer is disconnected
        /// </summary>
        Disconnected,

        /// <summary>
        /// Indicates peer is connecting
        /// </summary>
        Connecting,

        /// <summary>
        /// Indicates connect is being acknowledged
        /// </summary>
        AcknowledgingConnect,

        /// <summary>
        /// Indicates connection is pending
        /// </summary>
        ConnectionPending,

        /// <summary>
        /// Indicates connection is succeed
        /// </summary>
        ConnectingSucceeded,

        /// <summary>
        /// Indicates the peer is connected
        /// </summary>
        Connected,

        /// <summary>
        /// Indicates the peer is going to disconnect
        /// </summary>
        DisconnectLater,

        /// <summary>
        /// Indicates the peer is disconnecting
        /// </summary>
        Disconnecting,

        /// <summary>
        /// Indicates disconnect is being acknowledged
        /// </summary>
        AcknowledgingDisconnect,

        /// <summary>
        /// Indicates the peer is a zombie
        /// </summary>
        /// <remarks>
        /// The meaning zombie is that the peer haven't reacted in a while
        /// </remarks>
        Zombie,
    }
}
