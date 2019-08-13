using System.Collections.Generic;
using MugenMvvm.Binding.Core;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Resources.Components
{
    public interface IBindingTypeResolverComponent : IComponent<IBindingResourceResolver>
    {
        IReadOnlyList<BindingType> GetKnownTypes();
    }
}