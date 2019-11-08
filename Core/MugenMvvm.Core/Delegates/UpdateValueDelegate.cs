namespace MugenMvvm.Delegates
{
    public delegate TValue UpdateValueDelegate<in TItem, in TNewValue, TValue, in TState>(TItem item, TNewValue addValue, TValue currentValue, TState state);

    public delegate TValue UpdateValueDelegate<in TItem, in TNewValue, TValue>(TItem item, TNewValue addValue, TValue currentValue);
}