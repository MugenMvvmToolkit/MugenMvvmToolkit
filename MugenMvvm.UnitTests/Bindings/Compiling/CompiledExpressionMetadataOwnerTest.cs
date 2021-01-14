using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Compiling
{
    public class CompiledExpressionMetadataOwnerTest : MetadataOwnerTestBase
    {
        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new CompiledExpression(ConstantExpressionNode.EmptyString, metadata);
    }
}