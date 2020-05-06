namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestConverterClass
    {
        #region Properties

        public object? this[object index] => null;

        public object? Property { get; set; }

        public static object? PropertyStatic { get; set; }

        #endregion

        #region Methods

        public object? Method()
        {
            return null;
        }

        public static object? MethodStatic()
        {
            return null;
        }

        #endregion
    }
}