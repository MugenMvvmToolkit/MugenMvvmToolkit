namespace MugenMvvm.UnitTests.Binding.Parsing.Internal
{
    public class TestConverterClass
    {
        #region Properties

        public object? this[object index] => null;

        public object? Property { get; set; }

        public static object? PropertyStatic { get; set; }

        #endregion

        #region Methods

        public object? Method(object? arg) => null;

        public static object? MethodStatic(object? arg) => null;

        #endregion
    }
}