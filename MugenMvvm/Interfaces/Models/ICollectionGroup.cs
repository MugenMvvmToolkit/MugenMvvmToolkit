using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Models
{
    public interface ICollectionGroup<T>
    {
        IList<T> Items { get; }

        bool TryCleanup();
    }
}