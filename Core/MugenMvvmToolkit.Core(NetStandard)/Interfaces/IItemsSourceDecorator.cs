#region Copyright

// ****************************************************************************
// <copyright file="IItemsSourceDecorator.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IItemsSourceDecorator
    {
        [NotNull]
        IList<T> Decorate<T>([CanBeNull]object owner, [NotNull] IList<T> itemsSource);
    }
}
