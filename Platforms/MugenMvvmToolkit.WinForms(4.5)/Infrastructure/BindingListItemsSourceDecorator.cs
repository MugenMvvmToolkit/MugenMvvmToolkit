#region Copyright
// ****************************************************************************
// <copyright file="BindingListItemsSourceDecorator.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represnets the class that allows to wrap items source collection to <see cref="BindingListWrapper{T}" />.
    /// </summary>
    public sealed class BindingListItemsSourceDecorator : IItemsSourceDecorator
    {
        #region Implementation of IItemsSourceDecorator

        /// <summary>
        ///     Decorates items source collection.
        /// </summary>
        public IList<T> Decorate<T>(IList<T> itemsSource)
        {
            var notifiableCollection = itemsSource as SynchronizedNotifiableCollection<T>;
            if (notifiableCollection == null)
                return itemsSource;
            return new BindingListWrapper<T>(notifiableCollection);
        }

        #endregion
    }
}