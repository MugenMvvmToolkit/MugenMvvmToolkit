using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface ITargetExpressionInterceptor : IComponent<IBindingManager>
    {
        IExpressionNode InterceptTargetExpression(IExpressionNode targetExpression, IReadOnlyMetadataContext? metadata);
    }
}