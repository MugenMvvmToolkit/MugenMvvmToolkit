using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace MugenMvvm
{
    public static class BoxingExtensions
    {
        #region Fields

        public const int CacheSize = 50;

        private static readonly BoxingDelegate<bool> BoxBoolDelegate = Box;
        private static readonly BoxingDelegate<int> BoxIntDelegate = Box;

        public static readonly object TrueObject = true;
        public static readonly object FalseObject = false;

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(bool value)
        {
            if (value)
                return TrueObject;
            return FalseObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(byte value)
        {
            return Cache<byte>.Items[value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(sbyte value)
        {
            if (value < 0)
                return Cache<sbyte>.NegativeItems[-value];
            return Cache<sbyte>.Items[value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(ushort value)
        {
            if (value < CacheSize)
                return Cache<ushort>.Items[value];
            return value;
        }

        public static object Box(short value)
        {
            if (value < 0)
            {
                if (value > -CacheSize)
                    return Cache<short>.NegativeItems[-value];
            }
            else if (value < CacheSize)
                return Cache<short>.Items[value];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(uint value)
        {
            if (value < CacheSize)
                return Cache<uint>.Items[value];
            return value;
        }

        public static object Box(int value)
        {
            if (value < 0)
            {
                if (value > -CacheSize)
                    return Cache<int>.NegativeItems[-value];
            }
            else if (value < CacheSize)
                return Cache<int>.Items[value];

            return value;
        }

        public static object Box(ulong value)
        {
            if (value < CacheSize)
                return Cache<ulong>.Items[value];
            return value;
        }

        public static object Box(long value)
        {
            if (value < 0)
            {
                if (value > -CacheSize)
                    return Cache<long>.NegativeItems[-value];
            }
            else if (value < CacheSize)
                return Cache<long>.Items[value];

            return value;
        }

        public static object Box<T>(T value)
        {
            if (BoxBoolDelegate is BoxingDelegate<T> d1)
                return d1(value);
            if (BoxIntDelegate is BoxingDelegate<T> d2)
                return d2(value);
            return value;
        }

        #endregion

        #region Nested types

        private delegate object BoxingDelegate<T>(T value);

        internal static class Cache<T>
        {
            #region Fields

            public static readonly object[] Items = GenerateItems(false);
            public static readonly object[] NegativeItems = GenerateItems(true);

            #endregion

            #region Methods

            private static object[] GenerateItems(bool negative)
            {
                var cacheSize = GetCacheSize(negative);
                if (cacheSize == 0)
                    return Default.EmptyArray<object>();

                var items = new object[cacheSize];
                for (var i = 0; i < items.Length; i++)
                    items[i] = Convert.ChangeType(negative ? -i : i, typeof(T));
                return items;
            }

            private static int GetCacheSize(bool negative)
            {
                if (negative)
                {
                    if (typeof(T) == typeof(sbyte))
                        return -sbyte.MinValue + 1;
                    if (typeof(T) == typeof(short) || typeof(T) == typeof(int) || typeof(T) == typeof(long))
                        return CacheSize;
                    return 0;
                }

                if (typeof(T) == typeof(byte))
                    return byte.MaxValue + 1;
                if (typeof(T) == typeof(sbyte))
                    return sbyte.MaxValue + 1;

                if (typeof(T) == typeof(short) ||
                    typeof(T) == typeof(int) ||
                    typeof(T) == typeof(long) ||
                    typeof(T) == typeof(ushort) ||
                    typeof(T) == typeof(uint) ||
                    typeof(T) == typeof(ulong))
                    return CacheSize;
                return 0;
            }

            #endregion
        }

        #endregion
    }
}