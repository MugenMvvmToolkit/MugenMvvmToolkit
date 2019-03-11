namespace MugenMvvm.Interfaces.Collections
{
    public interface IObservableCollectionDecoratorManager<T>
    {
        IObservableCollection<T> Collection { get; }

        void RaiseAdded(IObservableCollectionDecorator<T> decorator, T item, int index);

        void RaiseReplaced(IObservableCollectionDecorator<T> decorator, T oldItem, T newItem, int index);

        void RaiseMoved(IObservableCollectionDecorator<T> decorator, T item, int oldIndex, int newIndex);

        void RaiseRemoved(IObservableCollectionDecorator<T> decorator, T item, int index);

        void RaiseReset(IObservableCollectionDecorator<T> decorator);

        void RaiseCleared(IObservableCollectionDecorator<T> decorator);
    }
}