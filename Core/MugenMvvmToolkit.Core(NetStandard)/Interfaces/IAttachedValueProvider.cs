#region Copyright

// ****************************************************************************
// <copyright file="IAttachedValueProvider.cs">
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

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IAttachedValueProvider
    {
        TValue AddOrUpdate<TItem, TValue>([NotNull] TItem item, [NotNull] string path, [CanBeNull] TValue addValue,
            [NotNull] UpdateValueDelegate<TItem, TValue, TValue, object> updateValueFactory, object state = null);

        TValue AddOrUpdate<TItem, TValue>([NotNull] TItem item, [NotNull] string path,
            [NotNull] Func<TItem, object, TValue> addValueFactory,
            [NotNull] UpdateValueDelegate<TItem, Func<TItem, object, TValue>, TValue, object> updateValueFactory,
            object state = null);

        TValue GetOrAdd<TItem, TValue>([NotNull] TItem item, [NotNull] string path,
            [NotNull] Func<TItem, object, TValue> valueFactory, object state);

        TValue GetOrAdd<TValue>([NotNull] object item, [NotNull] string path, [CanBeNull] TValue value);

        bool TryGetValue<TValue>([NotNull] object item, [NotNull] string path, out TValue value);

        TValue GetValue<TValue>([NotNull] object item, [NotNull] string path, bool throwOnError);

        void SetValue([NotNull] object item, [NotNull] string path, [CanBeNull] object value);

        bool Contains([NotNull] object item, [NotNull] string path);

        [NotNull]
        IList<KeyValuePair<string, object>> GetValues([NotNull] object item, [CanBeNull] Func<string, object, bool> predicate);

        bool Clear([NotNull] object item);

        bool Clear([NotNull] object item, [NotNull] string path);
    }
}
