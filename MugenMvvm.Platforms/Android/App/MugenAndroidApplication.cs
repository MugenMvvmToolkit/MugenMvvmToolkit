using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using MugenMvvm.Enums;

namespace MugenMvvm.Android.App
{
    public abstract class MugenAndroidApplication : Application
    {
        #region Constructors

        protected MugenAndroidApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected MugenAndroidApplication()
        {
        }

        #endregion

        #region Methods

        public override void OnTrimMemory(TrimMemory level)
        {
            if (level == TrimMemory.UiHidden)
            {
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivating, null);
                MugenService.Application.OnLifecycleChanged(ApplicationLifecycleState.Deactivated, null);
            }

            base.OnTrimMemory(level);
        }

        #endregion
    }
}