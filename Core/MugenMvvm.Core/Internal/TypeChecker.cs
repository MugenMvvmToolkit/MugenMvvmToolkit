using System.Runtime.CompilerServices;
using MugenMvvm.Extensions;

namespace MugenMvvm.Internal
{
    public static class TypeChecker
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullable<T>()
        {
            return GenericChecker<T>.IsNullableType;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValueType<T>()
        {
            return GenericChecker<T>.IsValueType;
        }

        #endregion

        #region Nested types

        private static class GenericChecker<T>
        {
            #region Fields

            public static readonly bool IsNullableType = BoxingExtensions.Box(default(T)) == null;
            public static readonly bool IsValueType = typeof(T).IsValueType;

            #endregion
        }

        #endregion
    }
}