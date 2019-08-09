using MugenMvvm.Binding.Interfaces.Parsing.Nodes;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Nodes;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParserResult : IMetadataOwner<IReadOnlyMetadataContext>
    {
        IExpressionNode TargetExpression { get; }

        IExpressionNode? SourceExpression { get; }

        BindingParserParameter[] Parameters { get; }
    }
}