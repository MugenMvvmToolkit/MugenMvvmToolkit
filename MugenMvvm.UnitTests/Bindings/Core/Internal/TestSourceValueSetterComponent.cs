using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestSourceValueSetterComponent : ISourceValueSetterComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public TrySetSourceValueDelegate? TrySetSourceValue { get; set; }

        #endregion

        #region Implementation of interfaces

        bool ISourceValueSetterComponent.TrySetSourceValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata) =>
            TrySetSourceValue?.Invoke(binding, targetMember, value, metadata) ?? false;

        #endregion

        #region Nested types

        public delegate bool TrySetSourceValueDelegate(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}