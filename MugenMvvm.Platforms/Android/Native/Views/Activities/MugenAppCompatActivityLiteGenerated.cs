using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Java.Interop;
using MugenMvvm.Android.Native.Interfaces.Views;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native.Views.Activities
{
    // Metadata.xml XPath class reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']"
    [Register("com/mugen/mvvm/views/activities/MugenAppCompatActivity", DoNotGenerateAcw = true)]
    public partial class MugenAppCompatActivityLite : Activity, INativeActivityView
    {
        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/views/activities/MugenAppCompatActivity", typeof(MugenAppCompatActivityLite));

        private static Delegate cb_getActivity;

        private static Delegate cb_getState;

        private static Delegate cb_setState_Ljava_lang_Object_;

        private static Delegate cb_getViewId;

        private static Delegate? cb_getContext;

        // Metadata.xml XPath constructor reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']/constructor[@name='MugenAppCompatActivity' and count(parameter)=0]"
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

        protected MugenAppCompatActivityLite(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public virtual unsafe Object Activity
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']/method[@name='getActivity' and count(parameter)=0]"
            [Register("getActivity", "()Ljava/lang/Object;", "GetGetActivityHandler")]
            get
            {
                const string __id = "getActivity.()Ljava/lang/Object;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe Context? Context
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenActivity']/method[@name='getContext' and count(parameter)=0]"
            [Register("getContext", "()Landroid/content/Context;", "GetGetContextHandler")]
            get
            {
                const string __id = "getContext.()Landroid/content/Context;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Context>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe Object State
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']/method[@name='getState' and count(parameter)=0]"
            [Register("getState", "()Ljava/lang/Object;", "GetGetStateHandler")]
            get
            {
                const string __id = "getState.()Ljava/lang/Object;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']/method[@name='setState' and count(parameter)=1 and parameter[1][@type='java.lang.Object']]"
            [Register("setState", "(Ljava/lang/Object;)V", "GetSetState_Ljava_lang_Object_Handler")]
            set
            {
                const string __id = "setState.(Ljava/lang/Object;)V";
                var __args = stackalloc JniArgumentValue[1];
                __args[0] = new JniArgumentValue(value == null ? IntPtr.Zero : value.Handle);
                _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, __args);
            }
        }

        public override JniPeerMembers JniPeerMembers => _members;

        public virtual unsafe int ViewId
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.activities']/class[@name='MugenAppCompatActivity']/method[@name='getViewId' and count(parameter)=0]"
            [Register("getViewId", "()I", "GetGetViewIdHandler")]
            get
            {
                const string __id = "getViewId.()I";
                var __rm = _members.InstanceMethods.InvokeVirtualInt32Method(__id, this, null);
                return __rm;
            }
        }

        protected override IntPtr ThresholdClass => _members.JniPeerType.PeerReference.Handle;

        protected override Type ThresholdType => _members.ManagedPeerType;

        internal static IntPtr class_ref => _members.JniPeerType.PeerReference.Handle;
#pragma warning disable 0169
        private static Delegate GetGetContextHandler()
        {
            if (cb_getContext == null)
                cb_getContext = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_L)n_GetContext);
            return cb_getContext;
        }

        private static IntPtr n_GetContext(IntPtr jnienv, IntPtr native__this)
        {
            var __this = GetObject<MugenActivity>(jnienv, native__this, JniHandleOwnership.DoNotTransfer)!;
            return JNIEnv.ToLocalJniHandle(__this.Context);
        }

        private static Delegate GetGetActivityHandler()
        {
            if (cb_getActivity == null)
                cb_getActivity = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>)n_GetActivity);
            return cb_getActivity;
        }

        private static IntPtr n_GetActivity(IntPtr jnienv, IntPtr native__this)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.Activity);
        }

        private static Delegate GetGetStateHandler()
        {
            if (cb_getState == null)
                cb_getState = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>)n_GetState);
            return cb_getState;
        }

        private static IntPtr n_GetState(IntPtr jnienv, IntPtr native__this)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.State);
        }

        private static Delegate GetSetState_Ljava_lang_Object_Handler()
        {
            if (cb_setState_Ljava_lang_Object_ == null)
                cb_setState_Ljava_lang_Object_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>)n_SetState_Ljava_lang_Object_);
            return cb_setState_Ljava_lang_Object_;
        }

        private static void n_SetState_Ljava_lang_Object_(IntPtr jnienv, IntPtr native__this, IntPtr native_tag)
        {
            MugenAppCompatActivityLite __this = GetObject<MugenAppCompatActivityLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Object tag = GetObject<Object>(native_tag, JniHandleOwnership.DoNotTransfer);
            __this.State = tag;
        }

        private static Delegate GetGetViewIdHandler()
        {
            if (cb_getViewId == null)
                cb_getViewId = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, int>)n_GetViewId);
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