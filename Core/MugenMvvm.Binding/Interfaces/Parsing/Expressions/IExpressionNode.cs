using MugenMvvm.Binding.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Expressions
{
    public interface IExpressionNode
    {
        ExpressionNodeType ExpressionType { get; }

        IExpressionNode Accept(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata = null);
    }
}