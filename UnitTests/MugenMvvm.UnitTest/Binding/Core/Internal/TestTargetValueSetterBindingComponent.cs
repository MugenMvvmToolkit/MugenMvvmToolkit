using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestTargetValueSetterBindingComponent : ITargetValueSetterBindingComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public TrySetTargetValueDelegate? TrySetTargetValue { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ITargetValueSetterBindingComponent.TrySetTargetValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            return TrySetTargetValue?.Invoke(binding, targetMember, value, metadata) ?? false;
        }

        #endregion

        #region Nested types

        public delegate bool TrySetTargetValueDelegate(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}