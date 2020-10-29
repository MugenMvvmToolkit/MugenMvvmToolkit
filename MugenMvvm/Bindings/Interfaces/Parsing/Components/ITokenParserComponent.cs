using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Bindings.Interfaces.Parsing.Components
{
    public interface ITokenParserComponent : IComponent<IExpressionParser>
    {
        IExpressionNode? TryParse(ITokenParserContext context, IExpressionNode? expression);
    }
}