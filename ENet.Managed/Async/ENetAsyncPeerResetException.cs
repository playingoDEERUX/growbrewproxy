using System;

namespace ENet.Managed.Async
{
    public sealed class ENetAsyncPeerResetException : Exception
    {
        public ENetAsyncPeerResetException() { }
        public ENetAsyncPeerResetException(string message) : base(message) { }
        public ENetAsyncPeerResetException(string message, Exception innerException) : base(message, innerException) { }
    }
}
