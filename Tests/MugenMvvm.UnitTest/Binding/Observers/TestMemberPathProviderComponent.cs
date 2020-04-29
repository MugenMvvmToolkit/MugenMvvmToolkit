using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Observers
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