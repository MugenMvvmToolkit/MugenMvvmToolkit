using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ISourceExpressionInterceptor : IComponent<IBindingManager>
    {
        IExpressionNode InterceptSourceExpression(IExpressionNode sourceExpression, IReadOnlyMetadataContext? metadata);
    }
}