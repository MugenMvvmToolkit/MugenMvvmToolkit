using MugenMvvm.Binding.Interfaces.Parsing.Nodes;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IExpressionVisitor
    {
        bool IsPostOrder { get; }

        IExpressionNode? Visit(IExpressionNode node);
    }
}