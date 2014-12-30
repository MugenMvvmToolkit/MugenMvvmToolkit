#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceDecorator.cs">
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

using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represnets the interface that allows to decorate items source collection.
    /// </summary>
    public interface IItemsSourceDecorator
    {
        /// <summary>
        ///     Decorates items source collection.
        /// </summary>
        [NotNull]
        IList<T> Decorate<T>([NotNull] IList<T> itemsSource);
    }
}