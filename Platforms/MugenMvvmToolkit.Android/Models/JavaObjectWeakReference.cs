#region Copyright

// ****************************************************************************
// <copyright file="JavaObjectWeakReference.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Runtime.CompilerServices;
using Android.Runtime;

namespace MugenMvvmToolkit.Android.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal class JavaObjectWeakReference : WeakReference
    {
        #region Constructors

        public JavaObjectWeakReference(IJavaObject item)
            : base(item)
        {
        }

        #endregion

        #region Overrides of WeakReference

        public override bool IsAlive => Target != null;

        public override object Target
        {
            get
            {
                var target = (IJavaObject)base.Target;
                if (target == null)
                    return null;
                if (target.Handle == IntPtr.Zero)
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

    internal sealed class JavaObjectWeakReferenceWeakTable : JavaObjectWeakReference
    {
        #region Fields

        private readonly int _hash;

        #endregion

        #region Constructors

        public JavaObjectWeakReferenceWeakTable(IJavaObject item)
            : base(item)
        {
            _hash = RuntimeHelpers.GetHashCode(item);
        }

        #endregion

        #region Overrides of WeakReference

        public override int GetHashCode()
        {
            return _hash;
        }

        #endregion
    }
}