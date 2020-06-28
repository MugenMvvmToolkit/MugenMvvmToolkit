using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Binding.Interfaces.Observation.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Observation.Internal
{
    public class TestMemberPathProviderComponent : IMemberPathProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<object, Type, IReadOnlyMetadataContext?, IMemberPath?>? TryGetMemberPath { get; set; }

        #endregion

        #region Implementation of interfaces

        IMemberPath? IMemberPathProviderComponent.TryGetMemberPath<TPath>(in TPath path, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMemberPath?.Invoke(path!, typeof(TPath), metadata);
        }

        #endregion
    }
}