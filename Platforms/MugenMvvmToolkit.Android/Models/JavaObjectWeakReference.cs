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

namespace MugenMvvmToolkit.Android.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JavaObjectWeakReference : WeakReference
    {
        #region Fields

        private readonly Java.Lang.Ref.WeakReference _nativeRef;

        #endregion

        #region Constructors

        public JavaObjectWeakReference(Java.Lang.Object item)
            : base(item, true)
        {
            _nativeRef = new Java.Lang.Ref.WeakReference(item);
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
                var target = (Java.Lang.Object)base.Target;
                if (target == null)
                    return null;
                if (target.Handle == IntPtr.Zero)
                {
                    base.Target = null;
                    return null;
                }
                if (_nativeRef.Get() == null)
                {
                    base.Target = null;
                    return null;
                }
                return target;
            }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}
