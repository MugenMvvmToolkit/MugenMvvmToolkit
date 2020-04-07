using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberManager : IComponentOwner<IMemberManager>, IComponent<IBindingManager>
    {
        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetMembers<TRequest>(in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}