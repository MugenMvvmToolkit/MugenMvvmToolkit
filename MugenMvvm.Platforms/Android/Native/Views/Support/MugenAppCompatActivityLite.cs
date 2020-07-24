using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Java.Interop;
using MugenMvvm.Android.Native.Interfaces.Views;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native.Views.Support
{
    [Register("com/mugen/mvvm/views/support/MugenAppCompatActivity", DoNotGenerateAcw = true)]
    public class MugenAppCompatActivityLite : Activity, INativeActivityView
    {
        #region Fields

        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/views/support/MugenAppCompatActivity", typeof(MugenAppCompatActivityLite));

        private static Delegate cb_getActivity;

        private static Delegate cb_getViewId;

        private static Delegate cb_getTag_I;

        private static Delegate cb_setTag_ILjava_lang_Object_;

        #endregion

        #region Constructors

        protected MugenAppCompatActivityLite(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        [Register(".ctor", "()V", "")]
        public unsafe MugenAppCompatActivityLite()
            : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            const string __id = "()V";

            if (Handle != IntPtr.Zero)
                return;

            var __r = _members.InstanceMethods.StartCreateInstance(__id, GetType(), null);
            SetHandle(__r.Handle, JniHandleOwnership.TransferLocalRef);
            _members.InstanceMethods.FinishCreateInstance(__id, this, null);
        }

        #endregion

        #region Properties

        internal static IntPtr class_ref => _members.JniPeerType.PeerReference.Handle;

        public override JniPeerMembers JniPeerMembers => _members;

        protected override IntPtr ThresholdClass => _members.JniPeerType.PeerReference.Handle;

        protected override Type ThresholdType => _members.ManagedPeerType;

        public virtual unsafe Context Activity
        {
            [Register("getActivity", "()Landroid/content/Context;", "GetGetActivityHandler")]
            get
            {
                const string __id = "getActivity.()Landroid/content/Context;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Context>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe int ViewId
        {
            [Register("getViewId", "()I", "GetGetViewIdHandler")]
            get
            {
                const string __id = "getViewId.()I";
                var __rm = _members.InstanceMethods.InvokeVirtualInt32Method(__id, this, null);
                return __rm;
            }
        }

        #endregion

        #region Implementation of interfaces

        [Register("getTag", "(I)Ljava/lang/Object;", "GetGetTag_IHandler")]
        public virtual unsafe Object GetTag(int id)
        {
            const string __id = "getTag.(I)Ljava/lang/Object;";
            var __args = stackalloc JniArgumentValue[1];
            __args[0] = new JniArgumentValue(id);
            var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, __args);
            return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
        }

        [Register("setTag", "(ILjava/lang/Object;)V", "GetSetTag_ILjava_lang_Object_Handler")]
        public virtual unsafe void SetTag(int id, Object state)
        {
            const string __id = "setTag.(ILjava/lang/Object;)V";
            var __args = stackalloc JniArgumentValue[2];
            __args[0] = new JniArgumentValue(id);
            __args[1] = new JniArgumentValue(state == null ? IntPtr.Zero : state.Handle);
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, __args);
        }

        #endregion

#pragma warning disable 0169
        private static Delegate GetGetActivityHandler()
        {
            if (cb_getActivity == null)
                cb_getActivity = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetActivity);
            return cb_getActivity;
        }

        private static IntPtr n_GetActivity(IntPtr jnienv, IntPtr native__this)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.Activity);
        }

        private static Delegate GetGetViewIdHandler()
        {
            if (cb_getViewId == null)
                cb_getViewId = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, int>) n_GetViewId);
            return cb_getViewId;
        }

        private static int n_GetViewId(IntPtr jnienv, IntPtr native__this)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return __this.ViewId;
        }

        private static Delegate GetGetTag_IHandler()
        {
            if (cb_getTag_I == null)
                cb_getTag_I = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, int, IntPtr>) n_GetTag_I);
            return cb_getTag_I;
        }

        private static IntPtr n_GetTag_I(IntPtr jnienv, IntPtr native__this, int id)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.GetTag(id));
        }

        private static Delegate GetSetTag_ILjava_lang_Object_Handler()
        {
            if (cb_setTag_ILjava_lang_Object_ == null)
                cb_setTag_ILjava_lang_Object_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, int, IntPtr>) n_SetTag_ILjava_lang_Object_);
            return cb_setTag_ILjava_lang_Object_;
        }

        private static void n_SetTag_ILjava_lang_Object_(IntPtr jnienv, IntPtr native__this, int id, IntPtr native_state)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Object state = GetObject<Object>(native_state, JniHandleOwnership.DoNotTransfer);
            __this.SetTag(id, state);
        }
#pragma warning restore 0169
    }
}