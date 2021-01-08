using System;
using Android.Runtime;
using Java.Interop;
using Object = Java.Lang.Object;

namespace MugenMvvm.Android.Native
{
    // Metadata.xml XPath class reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenInitializerBase']"
    [Register("com/mugen/mvvm/MugenInitializerBase", DoNotGenerateAcw = true)]
    public abstract class NativeMugenInitializerBase : Object
    {
        #region Fields

        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/MugenInitializerBase", typeof(NativeMugenInitializerBase));
        private static Delegate cb_initialize;
        private static Delegate cb_onTrimMemoryInternal_I;

        #endregion

        #region Constructors

        protected NativeMugenInitializerBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        // Metadata.xml XPath constructor reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenInitializerBase']/constructor[@name='MugenInitializerBase' and count(parameter)=0]"
        [Register(".ctor", "()V", "")]
        protected unsafe NativeMugenInitializerBase()
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

        #endregion

        #region Methods

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenInitializerBase']/method[@name='initialize' and count(parameter)=0]"
        [Register("initialize", "()V", "GetInitializeHandler")]
        protected abstract void Initialize();

        // Metadata.xml XPath method reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenInitializerBase']/method[@name='onTrimMemoryInternal' and count(parameter)=1 and parameter[1][@type='int']]"
        [Register("onTrimMemoryInternal", "(I)V", "GetOnTrimMemoryInternal_IHandler")]
        protected abstract void OnTrimMemory(int level);

        #endregion

#pragma warning disable 0169
        private static Delegate GetInitializeHandler()
        {
            if (cb_initialize == null)
                cb_initialize = JNINativeWrapper.CreateDelegate((_JniMarshal_PP_V) n_Initialize);
            return cb_initialize;
        }

        private static void n_Initialize(IntPtr jnienv, IntPtr native__this)
        {
            var __this = GetObject<NativeMugenInitializerBase>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.Initialize();
        }
#pragma warning restore 0169
#pragma warning disable 0169
        private static Delegate GetOnTrimMemoryInternal_IHandler()
        {
            if (cb_onTrimMemoryInternal_I == null)
                cb_onTrimMemoryInternal_I = JNINativeWrapper.CreateDelegate((_JniMarshal_PPI_V) n_OnTrimMemoryInternal_I);
            return cb_onTrimMemoryInternal_I;
        }

        private static void n_OnTrimMemoryInternal_I(IntPtr jnienv, IntPtr native__this, int p0)
        {
            var __this = GetObject<NativeMugenInitializerBase>(jnienv, native__this, JniHandleOwnership.DoNotTransfer);
            __this.OnTrimMemory(p0);
        }
#pragma warning restore 0169
    }
}