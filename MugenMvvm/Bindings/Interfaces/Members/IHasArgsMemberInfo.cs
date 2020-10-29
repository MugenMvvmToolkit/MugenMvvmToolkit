using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IHasArgsMemberInfo : IMemberInfo
    {
        ArgumentFlags ArgumentFlags { get; }

        IReadOnlyList<object?> GetArgs();
    }
}