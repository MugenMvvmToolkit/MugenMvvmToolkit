using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface IExpressionVisitor
    {
        ExpressionTraversalType TraversalType { get; }

        IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata);
    }
}