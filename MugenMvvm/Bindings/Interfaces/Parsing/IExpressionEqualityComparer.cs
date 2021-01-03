using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Bindings.Interfaces.Parsing
{
    public interface IExpressionEqualityComparer
    {
        bool? Equals(IExpressionNode x, IExpressionNode y);

        int? GetHashCode(IExpressionNode expression);
    }
}