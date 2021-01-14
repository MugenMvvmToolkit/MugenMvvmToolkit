using System;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Bindings.Members.Internal
{
    public class TestMethodMemberInfo : TestMemberInfoBase, IMethodMemberInfo
    {
        public Func<ItemOrIReadOnlyList<IParameterInfo>>? GetParameters { get; set; }

        public Func<ItemOrIReadOnlyList<Type>>? GetGenericArguments { get; set; }

        public Func<ItemOrArray<Type>, IMethodMemberInfo>? MakeGenericMethod { get; set; }

        public Func<IMethodMemberInfo>? GetGenericMethodDefinition { get; set; }

        public Func<object?, ItemOrArray<object?>, IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        public Func<EnumFlags<ArgumentFlags>, ItemOrIReadOnlyList<object?>, IReadOnlyMetadataContext?, IAccessorMemberInfo?>? TryGetAccessor { get; set; }

        public bool IsGenericMethod { get; set; }

        public bool IsGenericMethodDefinition { get; set; }

        ItemOrIReadOnlyList<IParameterInfo> IMethodMemberInfo.GetParameters() => GetParameters?.Invoke() ?? default;

        ItemOrIReadOnlyList<Type> IMethodMemberInfo.GetGenericArguments() => GetGenericArguments?.Invoke() ?? default;

        IMethodMemberInfo IMethodMemberInfo.GetGenericMethodDefinition() => GetGenericMethodDefinition?.Invoke()!;

        IMethodMemberInfo IMethodMemberInfo.MakeGenericMethod(ItemOrArray<Type> types) => MakeGenericMethod?.Invoke(types) ?? this;

        IAccessorMemberInfo? IMethodMemberInfo.TryGetAccessor(EnumFlags<ArgumentFlags> argumentFlags, ItemOrIReadOnlyList<object?> args, IReadOnlyMetadataContext? metadata) =>
            TryGetAccessor?.Invoke(argumentFlags, args, metadata);

        object? IMethodMemberInfo.Invoke(object? target, ItemOrArray<object?> args, IReadOnlyMetadataContext? metadata) => Invoke?.Invoke(target, args, metadata);
    }
}