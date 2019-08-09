using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IBindingParserContextProviderComponent : IComponent<IBindingManager>
    {
        IBindingParserContext? TryGetBindingParserContext();
    }
}