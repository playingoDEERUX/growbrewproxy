using System.Threading.Tasks;

namespace ENet.Managed.Internal.Threading
{
    internal readonly struct SharedStateLock<TSharedState>
        where TSharedState : class
    {
        private readonly FiloSemaphore m_Semaphore;
        private readonly TSharedState m_SharedState;

        public SharedStateLock(TSharedState sharedState)
        {
            m_SharedState = sharedState;
            m_Semaphore = new FiloSemaphore();
        }

        public SharedStateLockguard<TSharedState> Acquire()
        {
            m_Semaphore.Lock();
            return new SharedStateLockguard<TSharedState>(m_SharedState, m_Semaphore);
        }

        public async ValueTask<SharedStateLockguard<TSharedState>> AcquireAsync()
        {
            await m_Semaphore.LockAsync().ConfigureAwait(false);
            return new SharedStateLockguard<TSharedState>(m_SharedState, m_Semaphore);
        }
    }
}
