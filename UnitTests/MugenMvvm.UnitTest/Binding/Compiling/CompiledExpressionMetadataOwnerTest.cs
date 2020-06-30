using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTest.Metadata;

namespace MugenMvvm.UnitTest.Binding.Compiling
{
    public class CompiledExpressionMetadataOwnerTest : MetadataOwnerTestBase
    {
        #region Methods

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextManager? metadataContextManager)
        {
            return new CompiledExpression(ConstantExpressionNode.EmptyString, metadata, metadataContextManager);
        }

        #endregion
    }
}