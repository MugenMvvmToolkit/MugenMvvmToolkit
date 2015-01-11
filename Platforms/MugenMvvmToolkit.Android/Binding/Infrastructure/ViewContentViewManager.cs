#region Copyright

// ****************************************************************************
// <copyright file="ViewContentViewManager.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    internal class ViewContentViewManager : IContentViewManager
    {
        #region Implementation of IContentViewManager

        /// <summary>
        ///     Sets the specified content.
        /// </summary>
        public bool SetContent(object view, object content)
        {
            var viewGroup = view as ViewGroup;
            if (viewGroup == null)
                return false;
            if (content == null)
            {
                viewGroup.RemoveAllViews();
                return true;
            }

            var contentView = content as View;
            if (contentView == null)
                return false;
            if (viewGroup.ChildCount != 1 || viewGroup.GetChildAt(0) != contentView)
            {
                viewGroup.RemoveAllViews();
                viewGroup.AddView(contentView);
            }
            return true;
        }

        #endregion
    }
}