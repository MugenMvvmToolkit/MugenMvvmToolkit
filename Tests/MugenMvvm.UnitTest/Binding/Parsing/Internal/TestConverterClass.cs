namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    public class TestConverterClass
    {
        #region Fields

        public object? Field;

        #endregion

        #region Properties

        public object? this[object index] => null;

        public object? Property { get; set; }

        public object? PropertyStatic { get; set; }

        #endregion

        #region Methods

        public object? Method()
        {
            return null;
        }

        public object? MethodStatic()
        {
            return null;
        }

        #endregion
    }
}