using System;
using System.Reflection;

namespace MugenMvvm.Interfaces
{
    public interface IReflectionManager
    {
        Func<object?, object?[], object?> GetMethodDelegate(MethodInfo method);

        Delegate GetMethodDelegate(Type delegateType, MethodInfo method);
    }
}