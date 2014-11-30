#region Copyright
// ****************************************************************************
// <copyright file="ICollectionViewManager.cs">
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
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    public interface ICollectionViewManager
    {
        /// <summary>
        ///     Inserts an item to the specified index.
        /// </summary>
        void Insert([NotNull] object view, int index, object item);

        /// <summary>
        ///     Removes an item.
        /// </summary>
        void RemoveAt([NotNull] object view, int index);

        /// <summary>
        ///     Removes all items.
        /// </summary>
        void Clear([NotNull] object view);
    }
}