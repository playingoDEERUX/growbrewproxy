using System;

namespace ENet.Managed
{
    /// <summary>
    /// Reperesents an exception related to the (native) ENet.
    /// </summary>
    public sealed class ENetException : Exception
    {
        public ENetException(string message) : base(message) { }
        public ENetException(string message, Exception inner) : base(message, inner) { }
    }
}
