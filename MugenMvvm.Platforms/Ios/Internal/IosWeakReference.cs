using System;
using Foundation;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Ios.Extensions;
using ObjCRuntime;

namespace MugenMvvm.Ios.Internal
{
    public sealed class IosWeakReference : WeakReference, IWeakReference
    {
        #region Fields

        private IntPtr _targetHandle;

        #endregion

        #region Constructors

        public IosWeakReference(NSObject target) : base(target, true)
        {
            _targetHandle = target.Handle;
        }

        #endregion

        #region Properties

        public override object? Target
        {
            get
            {
                if (_targetHandle == IntPtr.Zero)
                    return null;
                var target = (NSObject?) base.Target;
                if (!target.IsAlive())
                {
                    target = Runtime.GetNSObject(_targetHandle);
                    base.Target = target;
                }

                return target;
            }
            set => ExceptionManager.ThrowNotSupported(nameof(Target));
        }

        #endregion

        #region Implementation of interfaces

        void IWeakReference.Release()
        {
        }

        #endregion

        #region Methods

        internal void OnDealloc()
        {
            _targetHandle = IntPtr.Zero;
            base.Target = null;
        }

        #endregion
    }
}