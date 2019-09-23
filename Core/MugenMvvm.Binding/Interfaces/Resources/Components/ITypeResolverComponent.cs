using System.Collections.Generic;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface ITypeResolverComponent : IComponent<IResourceResolver>
    {
        IReadOnlyList<KnownType> GetKnownTypes();
    }
}