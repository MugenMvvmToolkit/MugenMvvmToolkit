using MugenMvvm.Binding.Attributes;

namespace MugenMvvm.UnitTest.Binding.Parsing.Internal
{
    [ResourceBindingSyntaxExtension(ClassResource)]
    public class TestResourceExtensionClass
    {
        #region Fields

        public const string ClassResource = "c1";
        public const string MethodResource = "c2";
        public const string PropertyResource = "c3";
        public const string IndexerResource = "c4";

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