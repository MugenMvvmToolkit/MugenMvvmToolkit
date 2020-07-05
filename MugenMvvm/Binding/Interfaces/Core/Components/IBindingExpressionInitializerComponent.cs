using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingExpressionInitializerComponent : IComponent<IBindingManager>
    {
        void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context);
    }
}