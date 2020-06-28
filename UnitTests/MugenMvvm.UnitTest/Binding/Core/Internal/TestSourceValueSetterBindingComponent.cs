using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestSourceValueSetterBindingComponent : ISourceValueSetterBindingComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public TrySetSourceValueDelegate? TrySetSourceValue { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISourceValueSetterBindingComponent.TrySetSourceValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            return TrySetSourceValue?.Invoke(binding, targetMember, value, metadata) ?? false;
        }

        #endregion

        #region Nested types

        public delegate bool TrySetSourceValueDelegate(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}