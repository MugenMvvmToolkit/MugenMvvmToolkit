#region Copyright

// ****************************************************************************
// <copyright file="JNIWeakReference.cs">
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
using System.Threading;
using Android.Runtime;

namespace MugenMvvmToolkit.Android.Models
{
    //The two times faster than the JavaObjectWeakReference.
    //see http://developer.android.com/intl/ru/training/articles/perf-jni.html
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JNIWeakReference : WeakReference
    {
        #region Fields

        private IntPtr _weakHandle;

        #endregion

        #region Constructors

        public JNIWeakReference(IJavaObject item, IntPtr weakHandle)
            : base(item, true)
        {
            _weakHandle = weakHandle;
        }

        #endregion

        #region Methods

        private void ClearWeakReference(bool dispose)
        {
            var oldValue = Interlocked.Exchange(ref _weakHandle, IntPtr.Zero);
            if (oldValue == IntPtr.Zero)
                return;
            JNIEnv.DeleteWeakGlobalRef(oldValue);
            if (dispose)
                base.Target = null;
        }

        private bool IsJavaRefAlive()
        {
            IntPtr handle = IntPtr.Zero;
            try
            {
                handle = JNIEnv.NewLocalRef(_weakHandle);
                if (JNIEnv.IsSameObject(handle, IntPtr.Zero))
                {
                    Target = null;
                    return false;
                }
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    JNIEnv.DeleteLocalRef(handle);
            }
            return true;
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
                var target = (IJavaObject)base.Target;
                if (target == null)
                    return null;
                if (target.Handle == IntPtr.Zero)
                {
                    Target = null;
                    return null;
                }
                //From Android 4.0 (Ice Cream Sandwich) on, weak global references can be used like any other JNI references.
                if (PlatformExtensions.IsApiGreaterThanOrEqualTo14)
                {
                    if (JNIEnv.IsSameObject(_weakHandle, IntPtr.Zero))
                    {
                        Target = null;
                        return null;
                    }
                }
                else if (!IsJavaRefAlive())
                    return null;
                return target;
            }
            set
            {
                if (value != null)
                    throw new NotSupportedException();
                ClearWeakReference(true);
            }
        }

        ~JNIWeakReference()
        {
            ClearWeakReference(false);
        }

        #endregion
    }
}