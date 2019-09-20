using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Parsing
{
    public interface IBindingParser : IComponentOwner<IBindingParser>, IComponent<IBindingManager>
    {
        ItemOrList<BindingParserResult, IReadOnlyList<BindingParserResult>> Parse(string expression, IReadOnlyMetadataContext? metadata);
    }
}