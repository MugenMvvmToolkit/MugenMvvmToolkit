using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasEventListener<T> where T : class
    {
        void AddListener(T listener);

        void RemoveListener(T listener);

        IReadOnlyList<T> GetListeners();
    }
}