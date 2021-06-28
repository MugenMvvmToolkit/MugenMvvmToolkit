﻿using System.Runtime.CompilerServices;

namespace MugenMvvm.Internal
{
    public static class TypeChecker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullable<T>() => GenericChecker<T>.IsNullableType;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType<T>() => GenericChecker<T>.IsValueType;

        private static class GenericChecker<T>
        {
            public static readonly bool IsNullableType = default(T) == null;
            public static readonly bool IsValueType = typeof(T).IsValueType;
        }
    }
}