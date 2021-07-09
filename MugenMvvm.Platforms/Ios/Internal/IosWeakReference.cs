using System;
using System.Threading;
using Foundation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Ios.Extensions;
using ObjCRuntime;

namespace MugenMvvm.Ios.Internal
{
    public sealed class IosWeakReference : WeakReference, IWeakReference
    {
        private volatile IntPtr _targetHandle;

        public IosWeakReference(NSObject target) : base(target, true)
        {
            _targetHandle = target.Handle;
        }

        public override object? Target
        {
            get
            {
                if (_targetHandle == IntPtr.Zero)
                    return null;
                var target = (NSObject?)base.Target;
                if (!target.IsAlive())
                {
                    target = Runtime.GetNSObject(_targetHandle);
                    base.Target = target;
                }

                return target;
            }
            set => ExceptionManager.ThrowNotSupported(nameof(Target));
        }

        internal void OnDealloc()
        {
            Interlocked.Exchange(ref _targetHandle, IntPtr.Zero);
            base.Target = null;
        }

        void IWeakReference.Release()
        {
        }
    }
}