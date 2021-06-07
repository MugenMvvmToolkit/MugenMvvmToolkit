using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestTargetValueInterceptorComponent : ITargetValueInterceptorComponent, IHasPriority
    {
        public InterceptTargetValueDelegate? InterceptTargetValue { get; set; }

        public int Priority { get; set; }

        object? ITargetValueInterceptorComponent.InterceptTargetValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata) =>
            InterceptTargetValue?.Invoke(binding, sourceMember, value, metadata);

        public delegate object? InterceptTargetValueDelegate(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);
    }
}