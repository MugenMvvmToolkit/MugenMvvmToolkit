using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct MonitorLocker : IDisposable
    {
        private object? _locker;

        private MonitorLocker(object locker)
        {
            _locker = locker;
        }

        public bool IsEmpty => _locker == null;

        public void Dispose()
        {
            if (_locker != null)
            {
                Monitor.Exit(_locker);
                _locker = null;
            }
        }

        public static MonitorLocker Lock(object locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            var lockTaken = false;
            try
            {
                Monitor.Enter(locker, ref lockTaken);
                return new MonitorLocker(locker);
            }
            catch
            {
                if (lockTaken)
                    Monitor.Exit(locker);
                throw;
            }
        }
    }
}