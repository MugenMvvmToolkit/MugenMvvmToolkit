using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParser : IComponentOwner<IBindingParser>, IComponent<IBindingManager>
    {
        IReadOnlyList<BindingParserResult> Parse(string expression, IReadOnlyMetadataContext? metadata);
    }
}