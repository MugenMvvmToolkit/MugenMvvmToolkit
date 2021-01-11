using MugenMvvm.Bindings.Enums;
using MugenMvvm.Collections;
using MugenMvvm.Enums;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IHasArgsMemberInfo : IMemberInfo
    {
        EnumFlags<ArgumentFlags> ArgumentFlags { get; }

        ItemOrIReadOnlyList<object?> GetArgs();
    }
}