using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestSourceValueInterceptorBindingComponent : ISourceValueInterceptorBindingComponent, IHasPriority
    {
        #region Properties

        public InterceptSourceValueDelegate? InterceptSourceValue { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        object? ISourceValueInterceptorBindingComponent.InterceptSourceValue(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
        {
            return InterceptSourceValue?.Invoke(binding, sourceMember, value, metadata);
        }

        #endregion

        #region Nested types

        public delegate object? InterceptSourceValueDelegate(IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata);

        #endregion
    }
}