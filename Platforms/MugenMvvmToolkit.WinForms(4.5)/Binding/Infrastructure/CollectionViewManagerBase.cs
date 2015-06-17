#region Copyright

// ****************************************************************************
// <copyright file="CollectionViewManagerBase.cs">
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

#if WINFORMS
using MugenMvvmToolkit.WinForms.Binding.Interfaces;

namespace MugenMvvmToolkit.WinForms.Binding.Infrastructure
#elif ANDROID
using MugenMvvmToolkit.Android.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
#elif TOUCH
using MugenMvvmToolkit.iOS.Binding.Interfaces;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
#endif
{
    public abstract class CollectionViewManagerBase<TView, TViewItem> : ICollectionViewManager
    {
        #region Implementation of ICollectionViewManager

        void ICollectionViewManager.Insert(object view, int index, object item)
        {
            Insert((TView)view, index, (TViewItem)item);
        }

        void ICollectionViewManager.RemoveAt(object view, int index)
        {
            RemoveAt((TView)view, index);
        }

        void ICollectionViewManager.Clear(object view)
        {
            Clear((TView)view);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Inserts an item to the specified index.
        /// </summary>
        protected abstract void Insert(TView view, int index, TViewItem viewItem);

        /// <summary>
        ///     Removes an item.
        /// </summary>
        protected abstract void RemoveAt(TView view, int index);

        /// <summary>
        ///     Removes all items.
        /// </summary>
        protected abstract void Clear(TView view);

        #endregion
    }
}