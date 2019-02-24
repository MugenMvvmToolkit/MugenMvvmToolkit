using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasListeners<T> where T : class, IListener
    {
        void AddListener(T listener);

        void RemoveListener(T listener);

        IReadOnlyList<T> GetListeners();
    }
}