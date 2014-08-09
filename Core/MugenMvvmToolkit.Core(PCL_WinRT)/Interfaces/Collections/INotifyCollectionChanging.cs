#region Copyright
// ****************************************************************************
// <copyright file="INotifyCollectionChanging.cs">
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
using System.Collections.Specialized;
using System.ComponentModel;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    /// <summary>
    ///     Notifies listeners of dynamic changes, such as when items get added and removed or the whole list is refreshed.
    /// </summary>
    public interface INotifyCollectionChanging : INotifyCollectionChanged, INotifyPropertyChanged
    {
        /// <summary>
        ///     Occurs before the collection changes.
        /// </summary>
        event NotifyCollectionChangingEventHandler CollectionChanging;
    }
}