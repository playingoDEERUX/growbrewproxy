using System;

namespace ENet.Managed.Common
{
    public abstract class DisposableBase : IDisposable
    {
        ~DisposableBase() => Dispose(false);

        public bool Disposed { get; private set; } = false;

        public void Dispose()
        {
            if (Disposed)
                return;
            else
                Disposed = true;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
