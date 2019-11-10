using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMethodInfo : IObservableMemberInfo
    {
        bool IsGenericMethod { get; }

        bool IsGenericMethodDefinition { get; }

        IParameterInfo[] GetParameters();

        Type[] GetGenericArguments();

        IMethodInfo MakeGenericMethod(Type[] types);

        object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null);
    }
}