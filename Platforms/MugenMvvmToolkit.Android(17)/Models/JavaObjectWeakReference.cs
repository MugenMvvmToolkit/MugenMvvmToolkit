#region Copyright
// ****************************************************************************
// <copyright file="JavaObjectWeakReference.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

using Android.Views;
using MugenMvvmToolkit.Interfaces.Views;
using Object = Java.Lang.Object;
using WeakReference = System.WeakReference;

namespace MugenMvvmToolkit.Models
{
    //see https://bugzilla.xamarin.com/show_bug.cgi?id=16343
    internal sealed class JavaObjectWeakReference : WeakReference
    {
        #region Constructors

        public JavaObjectWeakReference(Object item, bool trackResurrection)
            : base(item, trackResurrection)
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
                var target = (Object)base.Target;
                if (target != null)
                {
                    if (target.IsAlive())
                    {
                        var activity = target as IActivityView;
                        if (activity == null)
                        {
                            var view = target as View;
                            if (view != null && view.Context != null)
                                activity = view.Context.GetActivity() as IActivityView;
                        }
                        if (activity == null || !activity.IsDestroyed)
                            return target;
                    }
                    base.Target = null;
                    return null;
                }
                return null;
            }
            set { base.Target = value; }
        }

        #endregion
    }
}