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
using Android.Views;
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JavaObjectWeakReference : WeakReference
    {
        #region Fields

        private bool _invalidContext;

        #endregion

        #region Constructors

        public JavaObjectWeakReference(IJavaObject item)
            : base(item, true)
        {

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
                if (target != null)
                {
                    if (target.IsAlive())
                    {
                        var activity = target as IActivityView;
                        if (activity == null && !_invalidContext)
                        {
                            var view = target as View;
                            try
                            {
                                if (view != null && view.Context != null)
                                    activity = view.Context.GetActivity() as IActivityView;
                            }
                            catch
                            {
                                _invalidContext = true;
                            }
                        }
                        if (activity == null || !activity.Mediator.IsDestroyed)
                            return target;
                    }
                    base.Target = null;
                    return null;
                }
                return null;
            }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}