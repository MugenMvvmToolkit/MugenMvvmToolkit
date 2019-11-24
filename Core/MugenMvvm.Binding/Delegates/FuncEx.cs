namespace MugenMvvm.Binding.Delegates
{
    internal delegate TReturn FuncEx<T1, TReturn>(in T1 arg1);//todo review

    internal delegate TReturn FuncEx<T1, T2, TReturn>(in T1 arg1, T2 arg2);

    internal delegate TReturn FuncEx<T1, T2, T3, TReturn>(in T1 arg1, T2 arg2, T3 arg3);
}