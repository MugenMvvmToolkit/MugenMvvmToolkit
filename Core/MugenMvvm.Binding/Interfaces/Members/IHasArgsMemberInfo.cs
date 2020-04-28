using System.Collections.Generic;
using MugenMvvm.Binding.Enums;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IHasArgsMemberInfo : IMemberInfo
    {
        ArgumentFlags ArgumentFlags { get; }

        IReadOnlyList<object?> GetArgs();
    }
}