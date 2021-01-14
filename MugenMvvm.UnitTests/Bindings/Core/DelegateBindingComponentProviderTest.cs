using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class DelegateBindingComponentProviderTest : UnitTestBase
    {
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
    }
}