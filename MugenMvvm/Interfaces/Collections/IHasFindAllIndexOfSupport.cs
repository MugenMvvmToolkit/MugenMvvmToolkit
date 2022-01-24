using System.Collections;
using MugenMvvm.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IHasFindAllIndexOfSupport : IEnumerable
    {
        void FindAllIndexOf(object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes);
    }
}