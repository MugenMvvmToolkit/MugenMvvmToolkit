using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Members
{
    public interface IMethodMemberInfo : IObservableMemberInfo//todo generic invoke
    {
        bool IsGenericMethod { get; }

        bool IsGenericMethodDefinition { get; }

        ItemOrIReadOnlyList<IParameterInfo> GetParameters();

        ItemOrIReadOnlyList<Type> GetGenericArguments();

        IMethodMemberInfo GetGenericMethodDefinition();

        IMethodMemberInfo MakeGenericMethod(ItemOrArray<Type> types);

        IAccessorMemberInfo? TryGetAccessor(EnumFlags<ArgumentFlags> argumentFlags, ItemOrIReadOnlyList<object?> args, IReadOnlyMetadataContext? metadata = null);

        object? Invoke(object? target, ItemOrArray<object?> args, IReadOnlyMetadataContext? metadata = null);
    }
}