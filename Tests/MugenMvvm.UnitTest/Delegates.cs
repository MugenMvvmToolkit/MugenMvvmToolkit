namespace MugenMvvm.UnitTest
{
    public delegate TResult FuncSingleRef<T1, T2, T3, T4, out TResult>(ref T1 arg1, T2 arg2, T3 arg3, T4 arg4);

    public delegate TResult FuncRef<T1, T2, T3, out TResult>(ref T1 arg1, ref T2 arg2, ref T3 arg3);

    public delegate TResult FuncRef<T1, T2, out TResult>(ref T1 arg1, ref T2 arg2);

    public delegate TResult FuncRef<T1, out TResult>(ref T1 arg1);
}