#region Copyright
// ****************************************************************************
// <copyright file="CollectionViewManagerBase.cs">
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

using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class CollectionViewManagerBase<TView, TItem> : ICollectionViewManager
    {
        #region Implementation of ICollectionViewManager

        void ICollectionViewManager.Insert(object view, int index, object item)
        {
            Insert((TView) view, index, (TItem) item);
        }

        void ICollectionViewManager.RemoveAt(object view, int index)
        {
            RemoveAt((TView) view, index);
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
        protected abstract void Insert(TView view, int index, TItem item);

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