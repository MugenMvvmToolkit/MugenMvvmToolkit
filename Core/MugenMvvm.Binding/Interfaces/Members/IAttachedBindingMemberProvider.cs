using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Infrastructure.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IAttachedBindingMemberProvider : IAttachableComponent<IBindingMemberProvider>, IDetachableComponent<IBindingMemberProvider>
    {
        IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext metadata);

        IReadOnlyList<AttachedMemberRegistration> GetMembers(Type type, IReadOnlyMetadataContext metadata);

        void Register(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext metadata);

        bool Unregister(Type type, string? name, IReadOnlyMetadataContext metadata);
    }
}