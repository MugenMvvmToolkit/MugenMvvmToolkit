using System;
using MugenMvvm.Binding.Interfaces.Observers.Components;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class TestMemberObserverProviderComponent : IMemberObserverProviderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Func<Type, object, Type, IReadOnlyMetadataContext?, MemberObserver>? TryGetMemberObserver { get; set; }

        #endregion

        #region Implementation of interfaces

        MemberObserver IMemberObserverProviderComponent.TryGetMemberObserver<TMember>(Type type, in TMember member, IReadOnlyMetadataContext? metadata)
        {
            return TryGetMemberObserver?.Invoke(type, member!, typeof(TMember), metadata) ?? default;
        }

        #endregion
    }
}