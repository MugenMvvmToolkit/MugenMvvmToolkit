using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IExpressionParserComponent : IComponent<IExpressionParser>
    {
        IExpressionNode? TryParse(IExpressionParserContext context, IExpressionNode? expression, IReadOnlyMetadataContext? metadata);
    }
}