using System.Collections;

namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableList : IObservableCollection, IList
    {
        void Move(int oldIndex, int newIndex);
    }
}