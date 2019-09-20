using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members.Components
{
    public interface IBindingMethodProviderComponent : IComponent<IBindingMemberProvider>
    {
        IReadOnlyList<IBindingMethodInfo> TryGetMethods(Type type, string name, IReadOnlyMetadataContext? metadata);
    }
}