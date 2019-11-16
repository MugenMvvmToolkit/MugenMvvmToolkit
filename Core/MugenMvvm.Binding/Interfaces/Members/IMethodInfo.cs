using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Members
{
    public interface IMethodInfo : IObservableMemberInfo
    {
        bool IsGenericMethod { get; }

        bool IsGenericMethodDefinition { get; }

        IReadOnlyList<IParameterInfo> GetParameters();

        IReadOnlyList<Type> GetGenericArguments();

        IMethodInfo MakeGenericMethod(Type[] types);

        object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null);
    }
}