using MugenMvvm.Bindings.Interfaces.Resources;

namespace MugenMvvm.UnitTests.Bindings.Resources.Internal
{
    public class TestDynamicResource : IDynamicResource
    {
        public object? Value { get; set; }
    }
}