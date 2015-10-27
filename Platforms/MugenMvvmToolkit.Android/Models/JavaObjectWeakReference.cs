#region Copyright

// ****************************************************************************
// <copyright file="JavaObjectWeakReference.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using Android.Runtime;
using Java.Lang.Ref;
using Object = Java.Lang.Object;
using WeakReference = System.WeakReference;

namespace MugenMvvmToolkit.Android.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JavaObjectWeakReference : WeakReference
    {
        #region Fields

        private static readonly IntPtr GetMethodId;
        private readonly Java.Lang.Ref.WeakReference _reference;

        #endregion

        #region Constructors

        static JavaObjectWeakReference()
        {
            GetMethodId = JNIEnv.GetMethodID(Java.Lang.Class.FromType(typeof(Reference)).Handle, "get", "()Ljava/lang/Object;");
        }

        public JavaObjectWeakReference(Object item)
            : base(item, true)
        {
            _reference = new Java.Lang.Ref.WeakReference(item);
        }

        #endregion

        #region Overrides of WeakReference

        public override bool IsAlive
        {
            get { return Target != null; }
        }

        public override object Target
        {
            get
            {
                var target = (Object)base.Target;
                if (target == null)
                    return null;
                if (target.Handle == IntPtr.Zero)
                {
                    base.Target = null;
                    return null;
                }
                //_reference.Get() very slow method, use JNI directly
                var handle = JNIEnv.CallObjectMethod(_reference.Handle, GetMethodId);
                JNIEnv.DeleteLocalRef(handle);
                if (handle == IntPtr.Zero)
                {
                    base.Target = null;
                    return null;
                }
                return target;
            }
            set
            {
                if (value != null)
                    throw new NotSupportedException();
                base.Target = null;
            }
        }

        #endregion
    }
}