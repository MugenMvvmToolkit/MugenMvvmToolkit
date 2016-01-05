#region Copyright

// ****************************************************************************
// <copyright file="ITrackingCollection.cs">
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Collections
{
    public interface ITrackingCollection : INotifyPropertyChanged, IEnumerable<TrackingEntity<object>>
#if PCL_WINRT
        , IReadOnlyCollection<TrackingEntity<object>>
#endif
    {
        [NotNull]
        IStateTransitionManager StateTransitionManager { get; set; }

#if !PCL_WINRT
        int Count { get; }
#endif

        bool HasChanges { get; }

        [Pure]
        bool Contains([NotNull] object item);

        [Pure]
        bool Contains<TEntity>([NotNull] Func<TrackingEntity<TEntity>, bool> predicate);

        [NotNull]
        IList<TEntity> Find<TEntity>([CanBeNull] Func<TrackingEntity<TEntity>, bool> predicate);

        [NotNull]
        IList<IEntityStateEntry> GetChanges(EntityState entityState = EntityState.Added | EntityState.Modified | EntityState.Deleted);

        [Pure]
        EntityState GetState([NotNull] object value);

        bool UpdateState([NotNull] object value, EntityState state);

        void Clear();

        [NotNull]
        ITrackingCollection Clone();
    }
}
