using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionPriorityProviderComponent : IComponent<IBindingManager>
    {
        bool TryGetPriority(IExpressionNode expression, out int priority);
    }
}