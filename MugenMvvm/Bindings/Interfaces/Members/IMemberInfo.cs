using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMemberInfo
    {
        string Name { get; }

        Type DeclaringType { get; }

        Type Type { get; }

        object? UnderlyingMember { get; }

        MemberType MemberType { get; }

        EnumFlags<MemberFlags> MemberFlags { get; }
    }
}