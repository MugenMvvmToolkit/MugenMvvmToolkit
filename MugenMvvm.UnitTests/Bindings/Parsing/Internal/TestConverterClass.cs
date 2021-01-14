namespace MugenMvvm.UnitTests.Bindings.Parsing.Internal
{
    public class TestConverterClass
    {
        public static object? PropertyStatic { get; set; }

        public object? Property { get; set; }

        public object? this[object index] => null;

        public static object? MethodStatic(object? arg) => null;

        public object? Method(object? arg) => null;
    }
}