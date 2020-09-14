using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.UnitTests.Binding.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Core
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

            var provider = new DelegateBindingComponentProvider<string>((s, b, arg3, arg4, arg5) =>
            {
                s.ShouldEqual(state);
                b.ShouldEqual(binding);
                arg3.ShouldEqual(target);
                arg4.ShouldEqual(source);
                arg5.ShouldEqual(DefaultMetadata);
                return result;
            }, state);
            provider.TryGetComponent(binding, target, source, DefaultMetadata).ShouldEqual(result);
        }

        #endregion
    }
}