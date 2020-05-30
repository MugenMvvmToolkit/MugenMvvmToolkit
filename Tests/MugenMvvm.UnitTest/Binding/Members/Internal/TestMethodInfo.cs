using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTest.Binding.Members.Internal
{
    public class TestMethodInfo : TestMemberInfoBase, IMethodInfo
    {
        #region Properties

        public bool IsGenericMethod { get; set; }

        public bool IsGenericMethodDefinition { get; set; }

        public Func<IReadOnlyList<IParameterInfo>>? GetParameters { get; set; }

        public Func<IReadOnlyList<Type>>? GetGenericArguments { get; set; }

        public Func<Type[], IMethodInfo>? MakeGenericMethod { get; set; }

        public Func<IMethodInfo>? GetGenericMethodDefinition { get; set; }

        public Func<object?, object?[], IReadOnlyMetadataContext?, object?>? Invoke { get; set; }

        #endregion

        #region Implementation of interfaces

        IReadOnlyList<IParameterInfo> IMethodInfo.GetParameters()
        {
            return GetParameters?.Invoke() ?? Default.Array<IParameterInfo>();
        }

        IReadOnlyList<Type> IMethodInfo.GetGenericArguments()
        {
            return GetGenericArguments?.Invoke() ?? Default.Array<Type>();
        }

        IMethodInfo IMethodInfo.GetGenericMethodDefinition()
        {
            return GetGenericMethodDefinition?.Invoke()!;
        }

        IMethodInfo IMethodInfo.MakeGenericMethod(Type[] types)
        {
            return MakeGenericMethod?.Invoke(types) ?? this;
        }

        object? IMethodInfo.Invoke(object? target, object?[] args, IReadOnlyMetadataContext? metadata)
        {
            return Invoke?.Invoke(target, args, metadata);
        }

        #endregion
    }
}