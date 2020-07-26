using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Java.Interop;
using MugenMvvm.Android.Native.Interfaces.Views;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native.Views.Activities
{
    [Register("com/mugen/mvvm/views/activities/MugenAppCompatActivity", DoNotGenerateAcw = true)]
    public partial class MugenAppCompatActivityLite : Activity, INativeActivityView
    {
        #region Fields

        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/views/activities/MugenAppCompatActivity", typeof(MugenAppCompatActivityLite));

        private static Delegate cb_getActivity;

        private static Delegate cb_getTag;

        private static Delegate cb_setTag_Ljava_lang_Object_;

        private static Delegate cb_getViewId;

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

        public virtual unsafe Object Tag
        {
            [Register("getTag", "()Ljava/lang/Object;", "GetGetTagHandler")]
            get
            {
                const string __id = "getTag.()Ljava/lang/Object;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
            [Register("setTag", "(Ljava/lang/Object;)V", "GetSetTag_Ljava_lang_Object_Handler")]
            set
            {
                const string __id = "setTag.(Ljava/lang/Object;)V";
                var __args = stackalloc JniArgumentValue[1];
                __args[0] = new JniArgumentValue(value == null ? IntPtr.Zero : value.Handle);
                _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, __args);
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
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetGetTagHandler()
        {
            if (cb_getTag == null)
                cb_getTag = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetTag);
            return cb_getTag;
        }

        private static IntPtr n_GetTag(IntPtr jnienv, IntPtr native__this)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.Tag);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetSetTag_Ljava_lang_Object_Handler()
        {
            if (cb_setTag_Ljava_lang_Object_ == null)
                cb_setTag_Ljava_lang_Object_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>) n_SetTag_Ljava_lang_Object_);
            return cb_setTag_Ljava_lang_Object_;
        }

        private static void n_SetTag_Ljava_lang_Object_(IntPtr jnienv, IntPtr native__this, IntPtr native_tag)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Object tag = GetObject<Object>(native_tag, JniHandleOwnership.DoNotTransfer);
            __this.Tag = tag;
        }
#pragma warning restore 0169
#pragma warning disable 0169
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
#pragma warning restore 0169
    }
}