using MugenMvvm.Binding.Attributes;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    [ResourceBindingSyntaxExtension(ClassResource)]
    public class TestResourceExtensionClass
    {
        #region Fields

        public object? Field;

        [ResourceBindingSyntaxExtension(FieldResource)]
        public object? FieldResourceExt;

        public const string ClassResource = "c1";
        public const string MethodResource = "c2";
        public const string FieldResource = "c3";
        public const string PropertyResource = "c4";
        public const string IndexerResource = "c5";

        #endregion

        #region Properties

        [ResourceBindingSyntaxExtension(IndexerResource)]
        public object? this[int index] => null;

        public object? this[object index] => null;

        public object? Property { get; set; }

        [ResourceBindingSyntaxExtension(PropertyResource)]
        public object? PropertyResourceExt { get; set; }

        #endregion

        #region Methods

        [ResourceBindingSyntaxExtension(MethodResource)]
        public object? MethodResourceExt()
        {
            return null;
        }

        public object? Method()
        {
            return null;
        }

        #endregion
    }
}