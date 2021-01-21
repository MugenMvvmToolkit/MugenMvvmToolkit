using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Java.Interop;
using MugenMvvm.Android.Native.Interfaces.Views;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native.Views.Fragments
{
    // Metadata.xml XPath class reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']"
    [Register("com/mugen/mvvm/views/fragments/MugenFragment", DoNotGenerateAcw = true)]
    public partial class MugenFragmentLite : Object, INativeFragmentView
    {
        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/views/fragments/MugenFragment", typeof(MugenFragmentLite));

        private static Delegate cb_getContext;

        private static Delegate cb_getFragment;

        private static Delegate cb_getState;

        private static Delegate cb_setState_Ljava_lang_Object_;

        private static Delegate cb_getView;

        private static Delegate cb_getViewId;

        private static Delegate cb_onCreate_Landroid_os_Bundle_;

        private static Delegate cb_onCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_;

        private static Delegate cb_onDestroy;

        private static Delegate cb_onDestroyView;

        private static Delegate cb_onPause;

        private static Delegate cb_onResume;

        private static Delegate cb_onSaveInstanceState_Landroid_os_Bundle_;

        private static Delegate cb_onStart;

        private static Delegate cb_onStop;

        // Metadata.xml XPath constructor reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/constructor[@name='MugenFragmentLite' and count(parameter)=0]"
        [Register(".ctor", "()V", "")]
        public unsafe MugenFragmentLite()
            : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            const string __id = "()V";

            if (Handle != IntPtr.Zero)
                return;

            var __r = _members.InstanceMethods.StartCreateInstance(__id, GetType(), null);
            SetHandle(__r.Handle, JniHandleOwnership.TransferLocalRef);
            _members.InstanceMethods.FinishCreateInstance(__id, this, null);
        }

        protected MugenFragmentLite(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        internal static IntPtr class_ref => _members.JniPeerType.PeerReference.Handle;

        public virtual unsafe Context Context
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getContext' and count(parameter)=0]"
            [Register("getContext", "()Landroid/content/Context;", "GetGetContextHandler")]
            get
            {
                const string __id = "getContext.()Landroid/content/Context;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Context>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe View View
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getView' and count(parameter)=0]"
            [Register("getView", "()Landroid/view/View;", "GetGetViewHandler")]
            get
            {
                const string __id = "getView.()Landroid/view/View;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<View>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public unsafe Activity Activity
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getActivity' and count(parameter)=0]"
            [Register("getActivity", "()Landroid/app/Activity;", "")]
            get
            {
                const string __id = "getActivity.()Landroid/app/Activity;";
                var __rm = _members.InstanceMethods.InvokeNonvirtualObjectMethod(__id, this, null);
                return GetObject<Activity>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe Object Fragment
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getFragment' and count(parameter)=0]"
            [Register("getFragment", "()Ljava/lang/Object;", "GetGetFragmentHandler")]
            get
            {
                const string __id = "getFragment.()Ljava/lang/Object;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        public virtual unsafe Object State
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getState' and count(parameter)=0]"
            [Register("getState", "()Ljava/lang/Object;", "GetGetStateHandler")]
            get
            {
                const string __id = "getState.()Ljava/lang/Object;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return GetObject<Object>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='setState' and count(parameter)=1 and parameter[1][@type='java.lang.Object']]"
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
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='getViewId' and count(parameter)=0]"
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

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onCreate' and count(parameter)=1 and parameter[1][@type='android.os.Bundle']]"
        [Register("onCreate", "(Landroid/os/Bundle;)V", "GetOnCreate_Landroid_os_Bundle_Handler")]
        public virtual unsafe void OnCreate(Bundle savedInstanceState)
        {
            const string __id = "onCreate.(Landroid/os/Bundle;)V";
            var __args = stackalloc JniArgumentValue[1];
            __args[0] = new JniArgumentValue(savedInstanceState == null ? IntPtr.Zero : savedInstanceState.Handle);
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, __args);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onCreateView' and count(parameter)=3 and parameter[1][@type='android.view.LayoutInflater'] and parameter[2][@type='android.view.ViewGroup'] and parameter[3][@type='android.os.Bundle']]"
        [Register("onCreateView", "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;",
            "GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler")]
        public virtual unsafe View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            const string __id = "onCreateView.(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;";
            var __args = stackalloc JniArgumentValue[3];
            __args[0] = new JniArgumentValue(inflater == null ? IntPtr.Zero : inflater.Handle);
            __args[1] = new JniArgumentValue(container == null ? IntPtr.Zero : container.Handle);
            __args[2] = new JniArgumentValue(savedInstanceState == null ? IntPtr.Zero : savedInstanceState.Handle);
            var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, __args);
            return GetObject<View>(__rm.Handle, JniHandleOwnership.TransferLocalRef);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onDestroy' and count(parameter)=0]"
        [Register("onDestroy", "()V", "GetOnDestroyHandler")]
        public virtual unsafe void OnDestroy()
        {
            const string __id = "onDestroy.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onDestroyView' and count(parameter)=0]"
        [Register("onDestroyView", "()V", "GetOnDestroyViewHandler")]
        public virtual unsafe void OnDestroyView()
        {
            const string __id = "onDestroyView.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onPause' and count(parameter)=0]"
        [Register("onPause", "()V", "GetOnPauseHandler")]
        public virtual unsafe void OnPause()
        {
            const string __id = "onPause.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onResume' and count(parameter)=0]"
        [Register("onResume", "()V", "GetOnResumeHandler")]
        public virtual unsafe void OnResume()
        {
            const string __id = "onResume.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onSaveInstanceState' and count(parameter)=1 and parameter[1][@type='android.os.Bundle']]"
        [Register("onSaveInstanceState", "(Landroid/os/Bundle;)V", "GetOnSaveInstanceState_Landroid_os_Bundle_Handler")]
        public virtual unsafe void OnSaveInstanceState(Bundle outState)
        {
            const string __id = "onSaveInstanceState.(Landroid/os/Bundle;)V";
            var __args = stackalloc JniArgumentValue[1];
            __args[0] = new JniArgumentValue(outState == null ? IntPtr.Zero : outState.Handle);
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, __args);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onStart' and count(parameter)=0]"
        [Register("onStart", "()V", "GetOnStartHandler")]
        public virtual unsafe void OnStart()
        {
            const string __id = "onStart.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm.views.fragments']/class[@name='MugenFragmentLite']/method[@name='onStop' and count(parameter)=0]"
        [Register("onStop", "()V", "GetOnStopHandler")]
        public virtual unsafe void OnStop()
        {
            const string __id = "onStop.()V";
            _members.InstanceMethods.InvokeVirtualVoidMethod(__id, this, null);
        }

#pragma warning disable 0169
        private static Delegate GetGetContextHandler()
        {
            if (cb_getContext == null)
                cb_getContext = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetContext);
            return cb_getContext;
        }

        private static IntPtr n_GetContext(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.Context);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetGetFragmentHandler()
        {
            if (cb_getFragment == null)
                cb_getFragment = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetFragment);
            return cb_getFragment;
        }

        private static IntPtr n_GetFragment(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.Fragment);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetGetStateHandler()
        {
            if (cb_getState == null)
                cb_getState = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetState);
            return cb_getState;
        }

        private static IntPtr n_GetState(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.State);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetSetState_Ljava_lang_Object_Handler()
        {
            if (cb_setState_Ljava_lang_Object_ == null)
                cb_setState_Ljava_lang_Object_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>) n_SetState_Ljava_lang_Object_);
            return cb_setState_Ljava_lang_Object_;
        }

        private static void n_SetState_Ljava_lang_Object_(IntPtr jnienv, IntPtr native__this, IntPtr native_tag)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Object tag = GetObject<Object>(native_tag, JniHandleOwnership.DoNotTransfer);
            __this.State = tag;
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetGetViewHandler()
        {
            if (cb_getView == null)
                cb_getView = JNINativeWrapper.CreateDelegate((Func<IntPtr, IntPtr, IntPtr>) n_GetView);
            return cb_getView;
        }

        private static IntPtr n_GetView(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.ToLocalJniHandle(__this.View);
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
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return __this.ViewId;
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnCreate_Landroid_os_Bundle_Handler()
        {
            if (cb_onCreate_Landroid_os_Bundle_ == null)
                cb_onCreate_Landroid_os_Bundle_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>) n_OnCreate_Landroid_os_Bundle_);
            return cb_onCreate_Landroid_os_Bundle_;
        }

        private static void n_OnCreate_Landroid_os_Bundle_(IntPtr jnienv, IntPtr native__this, IntPtr native_savedInstanceState)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Bundle savedInstanceState = GetObject<Bundle>(native_savedInstanceState, JniHandleOwnership.DoNotTransfer);
            __this.OnCreate(savedInstanceState);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler()
        {
            if (cb_onCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_ == null)
            {
                cb_onCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_ =
                    JNINativeWrapper.CreateDelegate(
                        (Func<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr>) n_OnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_);
            }

            return cb_onCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_;
        }

        private static IntPtr n_OnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_(IntPtr jnienv, IntPtr native__this, IntPtr native_inflater,
            IntPtr native_container,
            IntPtr native_savedInstanceState)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            LayoutInflater inflater = GetObject<LayoutInflater>(native_inflater, JniHandleOwnership.DoNotTransfer);
            ViewGroup container = GetObject<ViewGroup>(native_container, JniHandleOwnership.DoNotTransfer);
            Bundle savedInstanceState = GetObject<Bundle>(native_savedInstanceState, JniHandleOwnership.DoNotTransfer);
            var __ret = JNIEnv.ToLocalJniHandle(__this.OnCreateView(inflater, container, savedInstanceState));
            return __ret;
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnDestroyHandler()
        {
            if (cb_onDestroy == null)
                cb_onDestroy = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnDestroy);
            return cb_onDestroy;
        }

        private static void n_OnDestroy(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnDestroy();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnDestroyViewHandler()
        {
            if (cb_onDestroyView == null)
                cb_onDestroyView = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnDestroyView);
            return cb_onDestroyView;
        }

        private static void n_OnDestroyView(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnDestroyView();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnPauseHandler()
        {
            if (cb_onPause == null)
                cb_onPause = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnPause);
            return cb_onPause;
        }

        private static void n_OnPause(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnPause();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnResumeHandler()
        {
            if (cb_onResume == null)
                cb_onResume = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnResume);
            return cb_onResume;
        }

        private static void n_OnResume(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnResume();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnSaveInstanceState_Landroid_os_Bundle_Handler()
        {
            if (cb_onSaveInstanceState_Landroid_os_Bundle_ == null)
                cb_onSaveInstanceState_Landroid_os_Bundle_ = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr, IntPtr>) n_OnSaveInstanceState_Landroid_os_Bundle_);
            return cb_onSaveInstanceState_Landroid_os_Bundle_;
        }

        private static void n_OnSaveInstanceState_Landroid_os_Bundle_(IntPtr jnienv, IntPtr native__this, IntPtr native_outState)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            Bundle outState = GetObject<Bundle>(native_outState, JniHandleOwnership.DoNotTransfer);
            __this.OnSaveInstanceState(outState);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnStartHandler()
        {
            if (cb_onStart == null)
                cb_onStart = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnStart);
            return cb_onStart;
        }

        private static void n_OnStart(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnStart();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnStopHandler()
        {
            if (cb_onStop == null)
                cb_onStop = JNINativeWrapper.CreateDelegate((Action<IntPtr, IntPtr>) n_OnStop);
            return cb_onStop;
        }

        private static void n_OnStop(IntPtr jnienv, IntPtr native__this)
        {
            MugenFragmentLite __this = GetObject<MugenFragmentLite>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnStop();
        }
#pragma warning restore 0169

        static Delegate cb_setViewResourceId_I;
#pragma warning disable 0169
        static Delegate GetSetViewResourceId_IHandler ()
        {
            if (cb_setViewResourceId_I == null)
                cb_setViewResourceId_I = JNINativeWrapper.CreateDelegate ((_JniMarshal_PPI_V) n_SetViewResourceId_I);
            return cb_setViewResourceId_I;
        }

        static void n_SetViewResourceId_I (IntPtr jnienv, IntPtr native__this, int resourceId)
        {
            var __this = global::Java.Lang.Object.GetObject<global::MugenMvvm.Android.Native.Views.Fragments.FragmentWrapper> (jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.SetViewResourceId (resourceId);
        }
#pragma warning restore 0169

        [Register ("setViewResourceId", "(I)V", "GetSetViewResourceId_IHandler")]
        public virtual unsafe void SetViewResourceId (int resourceId)
        {
            const string __id = "setViewResourceId.(I)V";
            try {
                JniArgumentValue* __args = stackalloc JniArgumentValue [1];
                __args [0] = new JniArgumentValue (resourceId);
                _members.InstanceMethods.InvokeVirtualVoidMethod (__id, this, __args);
            } finally {
            }
        }
    }
}