using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface ITokenParserComponent : IComponent<IExpressionParser>
    {
        IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression);
    }
}