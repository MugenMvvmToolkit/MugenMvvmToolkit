using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Foundation;
using MugenMvvm.Ios.Constants;
using ObjCRuntime;

namespace MugenMvvm.Ios.Internal
{
    public sealed class IosAttachedValueHolder : NSObject
    {
        #region Fields

        public readonly IosWeakReference WeakReference;

        private SortedList<string, object?>? _values;
        private const int OBJC_ASSOCIATION_RETAIN_NONATOMIC = 1;
        private static readonly IntPtr KeyHandle = NSString.CreateNative(IosInternalConstants.AttachedHolderKey, false);

        #endregion

        #region Constructors

        private IosAttachedValueHolder(NSObject nativeObject)
        {
            WeakReference = new IosWeakReference(nativeObject);
        }

        #endregion

        #region Methods

        [DllImport(ObjCRuntime.Constants.ObjectiveCLibrary)]
        private static extern void objc_setAssociatedObject(IntPtr target, IntPtr key, IntPtr value, int policy);

        [DllImport(ObjCRuntime.Constants.ObjectiveCLibrary)]
        private static extern IntPtr objc_getAssociatedObject(IntPtr target, IntPtr key);

        public static IosAttachedValueHolder? Get(NSObject target, bool optional)
        {
            IntPtr intPtr;
            lock (target)
            {
                intPtr = objc_getAssociatedObject(target.Handle, KeyHandle);
                if (intPtr == IntPtr.Zero)
                {
                    if (optional)
                        return null;
                    var holder = new IosAttachedValueHolder(target);
                    objc_setAssociatedObject(target.Handle, KeyHandle, holder.Handle, OBJC_ASSOCIATION_RETAIN_NONATOMIC);
                    return holder;
                }
            }

            return Runtime.GetNSObject<IosAttachedValueHolder>(intPtr);
        }

        public SortedList<string, object?>? GetValues(bool optional)
        {
            if (optional)
                return _values;
            return _values ??= new SortedList<string, object?>(3, StringComparer.Ordinal);
        }

        [Export("dealloc")]
        private void Dealloc() => WeakReference.OnDealloc();

        #endregion
    }
}