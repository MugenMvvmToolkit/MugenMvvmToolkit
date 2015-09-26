#region Copyright

// ****************************************************************************
// <copyright file="GlobalViewParentListener.cs">
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

using Android.Views;
using Java.Lang;

namespace MugenMvvmToolkit.Android.Infrastructure
{
    public sealed class GlobalViewParentListener : Object, ViewGroup.IOnHierarchyChangeListener
    {
        #region Fields

        public static readonly GlobalViewParentListener Instance;

        #endregion

        #region Constructors

        static GlobalViewParentListener()
        {
            Instance = new GlobalViewParentListener();
        }

        private GlobalViewParentListener()
        {
        }

        #endregion

        #region Implementation of IOnHierarchyChangeListener

        public void OnChildViewAdded(View parent, View child)
        {
            child.ListenParentChange();
        }

        public void OnChildViewRemoved(View parent, View child)
        {
            child.ListenParentChange();
        }

        #endregion
    }
}
