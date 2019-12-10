namespace MugenMvvm.Delegates
{
    public delegate TReturn FuncIn<T1, TReturn>(in T1 arg1);

    public delegate TReturn FuncIn<T1, T2, TReturn>(in T1 arg1, T2 arg2);

    public delegate TReturn FuncIn<T1, T2, T3, TReturn>(in T1 arg1, T2 arg2, T3 arg3);
}