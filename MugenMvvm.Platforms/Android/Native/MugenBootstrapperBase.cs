using System;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native
{
    // Metadata.xml XPath class reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenBootstrapperBase']"
    [Register("com/mugen/mvvm/MugenBootstrapperBase", DoNotGenerateAcw = true)]
    public abstract class MugenBootstrapperBase : Object
    {
        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/MugenBootstrapperBase", typeof(MugenBootstrapperBase));

        private static Delegate cb_getFlags;
        private static Delegate cb_getRootActivity;
        private static Delegate cb_initialize;

        // Metadata.xml XPath constructor reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenBootstrapperBase']/constructor[@name='MugenBootstrapperBase' and count(parameter)=0]"
        [Register(".ctor", "()V", "")]
        public unsafe MugenBootstrapperBase()
            : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            const string __id = "()V";

            if (Handle != IntPtr.Zero)
                return;

            var __r = _members.InstanceMethods.StartCreateInstance(__id, GetType(), null);
            SetHandle(__r.Handle, JniHandleOwnership.TransferLocalRef);
            _members.InstanceMethods.FinishCreateInstance(__id, this, null);
        }

        protected MugenBootstrapperBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public override JniPeerMembers JniPeerMembers => _members;

        protected abstract int Flags
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenBootstrapperBase']/method[@name='getFlags' and count(parameter)=0]"
            [Register("getFlags", "()I", "GetGetFlagsHandler")]
            get;
        }

        protected virtual unsafe string RootActivity
        {
            // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenBootstrapperBase']/method[@name='getRootActivity' and count(parameter)=0]"
            [Register("getRootActivity", "()Ljava/lang/String;", "GetGetRootActivityHandler")]
            get
            {
                const string __id = "getRootActivity.()Ljava/lang/String;";
                var __rm = _members.InstanceMethods.InvokeVirtualObjectMethod(__id, this, null);
                return JNIEnv.GetString(__rm.Handle, JniHandleOwnership.TransferLocalRef);
            }
        }

        protected override IntPtr ThresholdClass => _members.JniPeerType.PeerReference.Handle;

        protected override Type ThresholdType => _members.ManagedPeerType;

        internal static IntPtr class_ref => _members.JniPeerType.PeerReference.Handle;

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenBootstrapperBase']/method[@name='initialize' and count(parameter)=0]"
        [Register("initialize", "()V", "GetInitializeHandler")]
        protected abstract void Initialize();
#pragma warning disable 0169
        private static Delegate GetGetFlagsHandler()
        {
            if (cb_getFlags == null)
                cb_getFlags = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_I)n_GetFlags);
            return cb_getFlags;
        }

        private static int n_GetFlags(IntPtr jnienv, IntPtr native__this)
        {
            var __this = GetObject<MugenBootstrapperBase>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return __this.Flags;
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetGetRootActivityHandler()
        {
            if (cb_getRootActivity == null)
                cb_getRootActivity = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_L)n_GetRootActivity);
            return cb_getRootActivity;
        }

        private static IntPtr n_GetRootActivity(IntPtr jnienv, IntPtr native__this)
        {
            var __this = GetObject<MugenBootstrapperBase>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            return JNIEnv.NewString(__this.RootActivity);
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetInitializeHandler()
        {
            if (cb_initialize == null)
                cb_initialize = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_V)n_Initialize);
            return cb_initialize;
        }

        private static void n_Initialize(IntPtr jnienv, IntPtr native__this)
        {
            var __this = GetObject<MugenBootstrapperBase>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.Initialize();
        }
#pragma warning restore 0169
    }
}