using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace MugenMvvm.Internal
{
    [StructLayout(LayoutKind.Auto)]
    public struct MonitorLocker : IDisposable
    {
        #region Fields

        private object? _locker;

        #endregion

        #region Constructors

        private MonitorLocker(object locker)
        {
            _locker = locker;
        }

        #endregion

        #region Properties

        public bool IsEmpty => _locker == null;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (_locker != null)
            {
                Monitor.Exit(_locker);
                _locker = null;
            }
        }

        #endregion

        #region Methods

        public static MonitorLocker Lock(object locker)
        {
            Should.NotBeNull(locker, nameof(locker));
            var lockTaken = false;
            Monitor.Enter(locker, ref lockTaken);
            if (lockTaken)
                return new MonitorLocker(locker);
            return default;
        }

        #endregion
    }
}