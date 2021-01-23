namespace ENet.Managed
{
    /// <summary>
    /// An interface that receives dispatches of ENet events.
    /// </summary>
    public interface IENetEventListener
    {
        void OnConnect(ENetPeer peer, uint data);
        void OnDisconnect(ENetPeer peer, uint data);
        void OnReceive(ENetPeer peer, ENetPacket packet, byte channelId);
    }
}
