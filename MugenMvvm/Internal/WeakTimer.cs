using System;
using System.Threading;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public static class WeakTimer
    {
        public static Timer Get<T>(T target, Action<T> action, IWeakReference? targetReference = null) where T : class
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(action, nameof(action));
            var closure = new TimerClosure<T>(targetReference ?? target.ToWeakReference(), action);
            closure.Timer = new Timer(o => ((TimerClosure<T>)o!).Execute(), closure, Timeout.Infinite, Timeout.Infinite);
            return closure.Timer;
        }

        private sealed class TimerClosure<T> where T : class
        {
            public Timer? Timer;
            private readonly IWeakReference _weakReference;
            private readonly Action<T> _action;

            public TimerClosure(IWeakReference weakReference, Action<T> action)
            {
                _weakReference = weakReference;
                _action = action;
            }

            public void Execute()
            {
                var target = (T?)_weakReference.Target;
                if (target == null)
                {
                    Timer?.Dispose();
                    Timer = null;
                }
                else
                    _action(target);
            }
        }
    }
}