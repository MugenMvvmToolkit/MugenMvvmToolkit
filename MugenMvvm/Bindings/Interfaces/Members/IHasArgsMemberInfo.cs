using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IHasArgsMemberInfo : IMemberInfo
    {
        EnumFlags<ArgumentFlags> ArgumentFlags { get; }

        IReadOnlyList<object?> GetArgs();
    }
}