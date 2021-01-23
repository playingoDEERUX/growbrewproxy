using System;

namespace ENet.Managed.Internal.Threading
{
    internal readonly struct SharedStateLockguard<TSharedState> : IDisposable
        where TSharedState : class
    {
        private readonly FiloSemaphore m_Semaphore;

        public readonly TSharedState SharedState;

        public SharedStateLockguard(TSharedState state, FiloSemaphore semaphore)
        {
            SharedState = state;
            m_Semaphore = semaphore;
        }

        public void Dispose()
        {
            m_Semaphore?.Release();
        }
    }
}
