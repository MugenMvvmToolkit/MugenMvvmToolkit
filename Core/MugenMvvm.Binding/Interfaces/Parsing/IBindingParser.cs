using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParser : IComponent<IBindingManager>
    {
        IBindingParserResult[] Parse(string expression, IReadOnlyMetadataContext? metadata);
    }
}