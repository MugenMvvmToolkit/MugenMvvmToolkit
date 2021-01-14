using MugenMvvm.Bindings.Attributes;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    [BindingMacros(ClassResource)]
    public class TestResourceExtensionClass
    {
        public const string ClassResource = "c1";
        public const string MethodResource = "c2";
        public const string PropertyResource = "c3";
        public const string IndexerResource = "c4";

        public object? Property { get; set; }

        [BindingMacros(PropertyResource)] public object? PropertyResourceExt { get; set; }

        [BindingMacros(IndexerResource)] public object? this[int index] => null;

        public object? this[object index] => null;

        [BindingMacros(MethodResource)]
        public object? MethodResourceExt(object? arg) => null;

        public object? Method(object? arg) => null;
    }
}