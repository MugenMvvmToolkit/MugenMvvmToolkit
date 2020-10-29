using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface IExpressionVisitor
    {
        bool IsPostOrder { get; }

        IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata);
    }
}