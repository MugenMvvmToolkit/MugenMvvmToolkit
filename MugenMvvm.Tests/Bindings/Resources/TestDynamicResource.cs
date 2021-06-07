using MugenMvvm.Bindings.Interfaces.Resources;

namespace MugenMvvm.Tests.Bindings.Resources
{
    public class TestDynamicResource : IDynamicResource
    {
        public object? Value { get; set; }
    }
}