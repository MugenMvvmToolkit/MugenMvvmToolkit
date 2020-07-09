using Java.Lang;
using MugenMvvm.Android.Native.Interfaces;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Android.Internal
{
    public sealed class AndroidNativeWeakReferenceCallback : Object, INativeWeakReferenceCallback
    {
        #region Implementation of interfaces

        public void OnWeakReferenceRemoved(Object p0)
        {
            (p0 as IWeakReference)?.Release();
        }

        #endregion
    }
}