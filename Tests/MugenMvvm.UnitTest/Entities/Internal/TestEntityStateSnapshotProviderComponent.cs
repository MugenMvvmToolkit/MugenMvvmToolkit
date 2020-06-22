using System;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Entities.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Entities.Internal
{
    public class TestEntityStateSnapshotProviderComponent : IEntityStateSnapshotProviderComponent, IHasPriority
    {
        #region Properties

        public Func<object, object?, Type, IReadOnlyMetadataContext?, IEntityStateSnapshot?>? TryGetSnapshot { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        IEntityStateSnapshot? IEntityStateSnapshotProviderComponent.TryGetSnapshot<TState>(object entity, in TState state, IReadOnlyMetadataContext? metadata)
        {
            return TryGetSnapshot?.Invoke(entity, state!, typeof(TState), metadata);
        }

        #endregion
    }
}