using System;
using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IBindingMemberInfo
    {
        bool IsAttached { get; }

        string Name { get; }

        Type Type { get; }

        object? Member { get; }

        BindingMemberType MemberType { get; }
    }
}