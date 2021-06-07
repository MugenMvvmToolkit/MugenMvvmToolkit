using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestSourceValueSetterComponent : ISourceValueSetterComponent, IHasPriority
    {
        public TrySetSourceValueDelegate? TrySetSourceValue { get; set; }

        public int Priority { get; set; }

        bool ISourceValueSetterComponent.TrySetSourceValue(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata) =>
            TrySetSourceValue?.Invoke(binding, targetMember, value, metadata) ?? false;

        public delegate bool TrySetSourceValueDelegate(IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata);
    }
}