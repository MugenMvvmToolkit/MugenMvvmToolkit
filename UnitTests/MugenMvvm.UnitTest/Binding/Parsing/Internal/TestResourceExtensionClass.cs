using MugenMvvm.Binding.Attributes;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    [BindingMacros(ClassResource)]
    public class TestResourceExtensionClass
    {
        #region Fields

        public const string ClassResource = "c1";
        public const string MethodResource = "c2";
        public const string PropertyResource = "c3";
        public const string IndexerResource = "c4";

        #endregion

        #region Properties

        [BindingMacros(IndexerResource)]
        public object? this[int index] => null;

        public object? this[object index] => null;

        public object? Property { get; set; }

        [BindingMacros(PropertyResource)]
        public object? PropertyResourceExt { get; set; }

        #endregion

        #region Methods

        [BindingMacros(MethodResource)]
        public object? MethodResourceExt(object? arg)
        {
            return null;
        }

        public object? Method(object? arg)
        {
            return null;
        }

        #endregion
    }
}