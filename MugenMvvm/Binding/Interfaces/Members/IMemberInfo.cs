using System;
using MugenMvvm.Bindings.Enums;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMemberInfo
    {
        string Name { get; }

        Type DeclaringType { get; }

        Type Type { get; }

        object? UnderlyingMember { get; }

        MemberType MemberType { get; }

        MemberFlags AccessModifiers { get; }
    }
}