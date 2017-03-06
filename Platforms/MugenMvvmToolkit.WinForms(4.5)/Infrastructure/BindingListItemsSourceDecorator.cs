#region Copyright

// ****************************************************************************
// <copyright file="BindingListItemsSourceDecorator.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.WinForms.Collections;

namespace MugenMvvmToolkit.WinForms.Infrastructure
{
    public sealed class BindingListItemsSourceDecorator : IItemsSourceDecorator
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public BindingListItemsSourceDecorator()
        {
        }

        #endregion

        #region Implementation of IItemsSourceDecorator

        public IList<T> Decorate<T>(object owner, IList<T> itemsSource)
        {
            var collection = itemsSource as INotifiableCollection<T>;
            if (collection == null)
                return itemsSource;
            return new BindingListWrapper<T>(collection);
        }

        #endregion
    }
}
