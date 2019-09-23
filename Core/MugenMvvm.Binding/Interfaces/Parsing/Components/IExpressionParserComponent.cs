using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
    }

    public interface IExpressionParserComponent<in TContext> : IExpressionParserComponent where TContext : class, ITokenExpressionParserContext
    {
        IExpressionNode? TryParse(TContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata);
    }
}