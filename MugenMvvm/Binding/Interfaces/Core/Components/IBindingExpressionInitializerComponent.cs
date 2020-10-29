using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingExpressionInitializerComponent : IComponent<IBindingManager>
    {
        void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context);
    }
}