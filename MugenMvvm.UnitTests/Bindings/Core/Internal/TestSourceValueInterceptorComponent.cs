using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestSourceValueInterceptorComponent : ISourceValueInterceptorComponent, IHasPriority
    {
        #region Properties

        public InterceptSourceValueDelegate? InterceptSourceValue { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ISourceValueInterceptorComponent.InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata) =>
            InterceptSourceValue?.Invoke(binding, sourceMember, value, metadata);

        #endregion

        #region Nested types

        public delegate object? InterceptSourceValueDelegate(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}