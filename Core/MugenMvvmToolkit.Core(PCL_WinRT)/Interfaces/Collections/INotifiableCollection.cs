#region Copyright

// ****************************************************************************
// <copyright file="INotifiableCollection.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    public interface INotifiableCollection : IList, INotifyCollectionChanging, ISuspendNotifications
    {
        void RaiseReset();

        void AddRange(IEnumerable collection);

        void RemoveRange(IEnumerable collection);

        event NotifyCollectionChangedEventHandler CollectionChangedUnsafe;
    }

    public interface INotifiableCollection<T> : IList<T>, INotifyCollectionChanging, ISuspendNotifications
    {
        void RaiseReset();

        void AddRange(IEnumerable<T> collection);

        void RemoveRange(IEnumerable<T> collection);

        void Update(IEnumerable<T> items);

        T[] ToArray();

        event NotifyCollectionChangedEventHandler CollectionChangedUnsafe;
    }
}
