using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestTargetValueInterceptorBindingComponent : ITargetValueInterceptorBindingComponent, IHasPriority
    {
        #region Properties

        public InterceptTargetValueDelegate? InterceptTargetValue { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ITargetValueInterceptorBindingComponent.InterceptTargetValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
        {
            return InterceptTargetValue?.Invoke(binding, sourceMember, value, metadata);
        }

        #endregion

        #region Nested types

        public delegate object? InterceptTargetValueDelegate(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}