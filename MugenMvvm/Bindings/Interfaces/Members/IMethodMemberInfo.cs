using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMethodMemberInfo : IObservableMemberInfo
    {
        bool IsGenericMethod { get; }

        bool IsGenericMethodDefinition { get; }

        IReadOnlyList<IParameterInfo> GetParameters();

        IReadOnlyList<Type> GetGenericArguments();

        IMethodMemberInfo GetGenericMethodDefinition();

        IMethodMemberInfo MakeGenericMethod(Type[] types);

        IAccessorMemberInfo? TryGetAccessor(EnumFlags<ArgumentFlags> argumentFlags, object?[]? args, IReadOnlyMetadataContext? metadata = null);

        object? Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata = null);
    }
}