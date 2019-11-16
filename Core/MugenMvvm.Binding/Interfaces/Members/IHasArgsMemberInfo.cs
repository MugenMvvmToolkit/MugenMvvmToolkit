using System.Collections.Generic;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IHasArgsMemberInfo : IMemberInfo
    {
        IReadOnlyList<object?> GetArgs();
    }
}