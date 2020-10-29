using MugenMvvm.Bindings.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Expressions
{
    public interface IExpressionNode
    {
        ExpressionNodeType ExpressionType { get; }

        IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null);
    }
}