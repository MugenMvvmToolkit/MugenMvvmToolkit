using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestMethodMemberInfo : TestMemberInfoBase, IMethodMemberInfo
    {
        #region Properties

        public bool IsGenericMethod { get; set; }

        public bool IsGenericMethodDefinition { get; set; }

        public Func<IReadOnlyList<IParameterInfo>>? GetParameters { get; set; }

        public Func<IReadOnlyList<Type>>? GetGenericArguments { get; set; }

        public Func<Type[], IMethodMemberInfo>? MakeGenericMethod { get; set; }

        public Func<IMethodMemberInfo>? GetGenericMethodDefinition { get; set; }

        public Func<object?, object?[], IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        public Func<ArgumentFlags, object?[]?, IReadOnlyMetadataContext?, IAccessorMemberInfo?>? TryGetAccessor { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IParameterInfo> IMethodMemberInfo.GetParameters()
        {
            return GetParameters?.Invoke() ?? Default.Array<IParameterInfo>();
        }

        IReadOnlyList<Type> IMethodMemberInfo.GetGenericArguments()
        {
            return GetGenericArguments?.Invoke() ?? Default.Array<Type>();
        }

        IMethodMemberInfo IMethodMemberInfo.GetGenericMethodDefinition()
        {
            return GetGenericMethodDefinition?.Invoke()!;
        }

        IMethodMemberInfo IMethodMemberInfo.MakeGenericMethod(Type[] types)
        {
            return MakeGenericMethod?.Invoke(types) ?? this;
        }

        IAccessorMemberInfo? IMethodMemberInfo.TryGetAccessor(ArgumentFlags argumentFlags, object?[]? args, IReadOnlyMetadataContext? metadata)
        {
            return TryGetAccessor?.Invoke(argumentFlags, args, metadata);
        }

        object? IMethodMemberInfo.Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata)
        {
            return Invoke?.Invoke(target, args, metadata);
        }

        #endregion
    }
}