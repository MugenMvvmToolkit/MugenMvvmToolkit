#region Copyright

// ****************************************************************************
// <copyright file="NativeObjectWeakReference.cs">
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
using Foundation;
using ObjCRuntime;

namespace MugenMvvmToolkit.iOS.Models
{
    internal sealed class NativeObjectWeakReference : WeakReference
    {
        #region Fields

        //NOTE Refcount(http://developer.xamarin.com/guides/ios/advanced_topics/newrefcount/) recreates NSObjects and breaks the weakreference.
        public bool IsInvalid;
        public readonly IntPtr Handle;

        #endregion

        #region Constructors

        public NativeObjectWeakReference(NSObject target)
            : base(target)
        {
            Handle = target.Handle;
        }

        #endregion

        #region Overrides of WeakReference

        public override bool IsAlive => Target != null;

        public override object Target
        {
            get
            {
                if (IsInvalid)
                    return null;
                var target = (NSObject)base.Target;
                if (!target.IsAlive())
                {
                    target = Runtime.GetNSObject(Handle);
                    base.Target = target;
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
