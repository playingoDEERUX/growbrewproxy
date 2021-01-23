using System;

namespace ENet.Managed.Async
{
    public sealed class ENetAsyncPeerDisconnectedException : Exception 
    {
        public ENetAsyncPeerDisconnectedException() { }
        public ENetAsyncPeerDisconnectedException(string message) : base(message) { }
        public ENetAsyncPeerDisconnectedException(string message, Exception inner) : base(message, inner) { }
    }
}
