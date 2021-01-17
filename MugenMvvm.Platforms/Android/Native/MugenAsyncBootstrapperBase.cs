using System;
using Android.Runtime;
using Java.Interop;

namespace MugenMvvm.Android.Native
{
    // Metadata.xml XPath class reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenAsyncBootstrapperBase']"
    [Register("com/mugen/mvvm/MugenAsyncBootstrapperBase", DoNotGenerateAcw = true)]
    public abstract class MugenAsyncBootstrapperBase : MugenBootstrapperBase
    {
        private static readonly JniPeerMembers _members = new XAPeerMembers("com/mugen/mvvm/MugenAsyncBootstrapperBase", typeof(MugenAsyncBootstrapperBase));

        // Metadata.xml XPath constructor reference: path="/api/package[@name='com.mugen.mvvm']/class[@name='MugenAsyncBootstrapperBase']/constructor[@name='MugenAsyncBootstrapperBase' and count(parameter)=0]"
        [Register(".ctor", "()V", "")]
        public unsafe MugenAsyncBootstrapperBase()
            : base(IntPtr.Zero, JniHandleOwnership.DoNotTransfer)
        {
            const string __id = "()V";

            if (Handle != IntPtr.Zero)
                return;

            var __r = _members.InstanceMethods.StartCreateInstance(__id, GetType(), null);
            SetHandle(__r.Handle, JniHandleOwnership.TransferLocalRef);
            _members.InstanceMethods.FinishCreateInstance(__id, this, null);
        }

        protected MugenAsyncBootstrapperBase(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        internal new static IntPtr class_ref => _members.JniPeerType.PeerReference.Handle;

        public override JniPeerMembers JniPeerMembers => _members;

        protected override IntPtr ThresholdClass => _members.JniPeerType.PeerReference.Handle;

        protected override Type ThresholdType => _members.ManagedPeerType;
    }
}