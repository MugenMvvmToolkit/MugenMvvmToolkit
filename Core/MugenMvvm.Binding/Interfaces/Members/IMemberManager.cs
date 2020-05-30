using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberManager : IComponentOwner<IMemberManager>, IComponent<IBindingManager>
    {
        ItemOrList<IMemberInfo, IReadOnlyList<IMemberInfo>> GetMembers<TRequest>(Type type, MemberType memberTypes, MemberFlags flags, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}