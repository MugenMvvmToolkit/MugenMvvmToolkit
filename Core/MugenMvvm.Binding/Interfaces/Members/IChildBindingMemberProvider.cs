using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IChildBindingMemberProvider : IHasPriority
    {
        IBindingMemberInfo? GetMember(IBindingMemberProvider provider, Type type, string name, IReadOnlyMetadataContext metadata);
    }
}