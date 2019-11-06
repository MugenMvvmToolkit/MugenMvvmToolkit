using System;
using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMemberInfo
    {
        string Name { get; }

        Type Type { get; }

        object? Member { get; }

        MemberType MemberType { get; }

        MemberFlags AccessModifiers { get; }
    }
}