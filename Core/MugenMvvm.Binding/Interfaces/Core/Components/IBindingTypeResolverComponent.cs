using System.Collections.Generic;
using MugenMvvm.Binding.Infrastructure.Core;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingTypeResolverComponent : IComponent<IBindingManager>
    {
        IReadOnlyList<BindingType> GetKnownTypes();
    }
}