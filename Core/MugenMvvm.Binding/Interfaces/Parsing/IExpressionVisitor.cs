using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionVisitor
    {
        bool IsPostOrder { get; }

        IExpressionNode? Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata);
    }
}