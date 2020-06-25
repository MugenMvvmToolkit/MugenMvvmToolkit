using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionInitializerComponent : IComponent<IBindingManager>
    {
        void Initialize(IBindingExpressionInitializerContext context);
    }
}