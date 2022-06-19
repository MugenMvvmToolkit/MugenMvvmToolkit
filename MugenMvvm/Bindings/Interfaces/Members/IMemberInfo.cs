using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMemberInfo : IHasName
    {
        Type DeclaringType { get; }

        Type Type { get; }

        object? UnderlyingMember { get; }

        MemberType MemberType { get; }

        EnumFlags<MemberFlags> MemberFlags { get; }
    }
}