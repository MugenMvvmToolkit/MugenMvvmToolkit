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

        public object? Method(object? arg)
        {
            return null;
        }

        public static object? MethodStatic(object? arg)
        {
            return null;
        }

        #endregion
    }
}