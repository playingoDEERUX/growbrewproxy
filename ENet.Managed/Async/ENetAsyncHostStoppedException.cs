using System;

namespace ENet.Managed.Async
{
    public sealed class ENetAsyncHostStoppedException : Exception
    {
        public ENetAsyncHostStoppedException(string message) : base(message) { }
        public ENetAsyncHostStoppedException(string message, Exception inner) : base(message, inner) { }
    }
}
