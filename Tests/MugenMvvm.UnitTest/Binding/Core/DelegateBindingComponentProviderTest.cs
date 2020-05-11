using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core
{
    public class DelegateBindingComponentProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void GetComponentShouldCallDelegate()
        {
            var binding = new TestBinding();
            var state = "s";
            var target = new object();
            var source = new object();
            var result = OneTimeBindingMode.Instance;

            var provider = new DelegateBindingComponentProvider<string>((in string s, IBinding b, object arg3, object? arg4, IReadOnlyMetadataContext? arg5) =>
            {
                s.ShouldEqual(state);
                b.ShouldEqual(binding);
                arg3.ShouldEqual(target);
                arg4.ShouldEqual(source);
                arg5.ShouldEqual(DefaultMetadata);
                return result;
            }, state);
            provider.GetComponent(binding, target, source, DefaultMetadata).ShouldEqual(result);
        }

        #endregion
    }
}