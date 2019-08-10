using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Parsing.Components
{
    public interface IBindingParserContextProviderComponent : IComponent<IBindingParser>
    {
        IBindingParserContext? TryGetBindingParserContext();
    }
}