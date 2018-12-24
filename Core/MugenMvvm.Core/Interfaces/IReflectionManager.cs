using System;
using System.Reflection;

namespace MugenMvvm.Interfaces
{
    public interface IReflectionManager
    {
        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);

        Func<object?, object?[], object?> GetMethodDelegate(MethodInfo method);

        Delegate GetMethodDelegate(Type delegateType, MethodInfo method);
    }
}