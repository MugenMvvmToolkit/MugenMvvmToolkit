using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Infrastructure.Members;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingMemberProvider//todo add expando/dynamic objects
    {
        IComponentCollection<IChildBindingMemberProvider> Providers { get; }

        IAttachedChildBindingMemberProvider AttachedChildBindingMemberProvider { get; set; }

        IBindingMemberInfo? GetMember(Type type, string name, IReadOnlyMetadataContext metadata);

        IBindingMemberInfo? GetRawMember(Type type, string name, IReadOnlyMetadataContext metadata);

        IReadOnlyList<AttachedMemberRegistration> GetAttachedMembers(Type type, IReadOnlyMetadataContext metadata);

        void Register(Type type, IBindingMemberInfo member, string? name, IReadOnlyMetadataContext metadata);

        bool Unregister(Type type, string? name, IReadOnlyMetadataContext metadata);
    }
}