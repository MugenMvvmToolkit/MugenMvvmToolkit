using System;
using Android.Runtime;
using MugenMvvm.Android.Native;
using MugenMvvm.Enums;

namespace MugenMvvm.Android.App
{
    public abstract class MugenInitializerBase : NativeMugenInitializerBase
    {
        #region Constructors

        protected MugenInitializerBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected MugenInitializerBase()
        {
        }

        #endregion

        #region Methods

        protected override void OnTrimMemory(int level)
        {
            if (level == 20) //UiHidden = 20
            {
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, null);
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null);
            }
        }

        #endregion
    }
}