using System;

namespace MugenMvvm.Delegates
{
    public delegate TReturn UpdateValueDelegate<TItem, in TValue, TState, TReturn>(TItem item, Func<TItem, TState, TReturn> factory, TValue currentValue, TState state);

    public delegate TReturn UpdateValueDelegate<in TItem, in TNewValue, in TValue, in TState, out TReturn>(TItem item, TNewValue newValue, TValue currentValue, TState state);
}