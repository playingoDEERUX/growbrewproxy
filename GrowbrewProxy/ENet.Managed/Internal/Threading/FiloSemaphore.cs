using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ENet.Managed.Internal.Threading
{
    internal sealed class FiloSemaphore
    {
        private sealed class AsyncWaiter : TaskCompletionSource<bool>
        {
            public AsyncWaiter() : base(TaskCreationOptions.RunContinuationsAsynchronously) { } 
            public void Set() => SetResult(true);
        }

        private enum State
        {
            Free,
            Locked,
        }

        [ThreadStatic]
        private static ManualResetEventSlim? ts_ResetEvent;

        private readonly object m_Sync = new object();
        private readonly Queue<object> m_WaitersQueue;
        private State m_State;

        internal int WaitersCount
        {
            get
            {
                lock (m_Sync)
                {
                    return m_WaitersQueue.Count;
                }
            }
        }

        internal bool IsLocked
        {
            get
            {
                lock (m_Sync)
                {
                    return m_State == State.Locked;
                }
            }
        }

        public FiloSemaphore()
        {
            m_State = State.Free;
            m_WaitersQueue = new Queue<object>();
        }

        public void Lock()
        {
            lock (m_Sync)
            {
                if (m_State == State.Free)
                {
                    m_State = State.Locked;
                    return;
                }

                if (ts_ResetEvent == null)
                {
                    ts_ResetEvent = new ManualResetEventSlim(false);
                }

                m_WaitersQueue.Enqueue(ts_ResetEvent);
            }

            ts_ResetEvent.Wait();
            ts_ResetEvent.Reset();
        }

        public ValueTask LockAsync()
        {
            AsyncWaiter asyncWaiter;

            lock (m_Sync)
            {
                if (m_State == State.Free)
                {
                    m_State = State.Locked;
                    return new ValueTask();
                }

                asyncWaiter = new AsyncWaiter();
                m_WaitersQueue.Enqueue(asyncWaiter);
            }

            return new ValueTask(asyncWaiter.Task);
        }

        public void Release()
        {
            lock (m_Sync)
            {
                if (!m_WaitersQueue.TryDequeue(out var waiter))
                {
                    m_State = State.Free;
                    return;
                }

                switch (waiter)
                {
                    case AsyncWaiter asyncWaiter:
                        asyncWaiter.Set();
                        break;
                    
                    case ManualResetEventSlim syncWaiter:
                        syncWaiter.Set();
                        break;
                    
                    default:
                        throw new InvalidOperationException();
                }
            }
        }
    }
}
