using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestSourceValueInterceptorComponent : ISourceValueInterceptorComponent, IHasPriority
    {
        public InterceptSourceValueDelegate? InterceptSourceValue { get; set; }

        public int Priority { get; set; }

        object? ISourceValueInterceptorComponent.InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata) =>
            InterceptSourceValue?.Invoke(binding, sourceMember, value, metadata);

        public delegate object? InterceptSourceValueDelegate(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);
    }
}