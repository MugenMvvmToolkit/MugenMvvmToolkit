using System;
using MugenMvvm.Interfaces.Internal;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Internal
{
    public sealed class AndroidWeakReference : WeakReference, IWeakReference
    {
        public AndroidWeakReference(Object target, bool trackResurrection) : base(target, trackResurrection)
        {
        }

        public override object? Target
        {
            get
            {
                var target = (Object?)base.Target;
                if (target == null || target.Handle == IntPtr.Zero)
                    return null;
                return target;
            }
            set => base.Target = value;
        }

        public void Release() => Target = null;
    }
}