using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static class BoxingExtensions
    {
        #region Fields

        public const int CacheSize = 50;
        public static readonly object TrueObject = true;
        public static readonly object FalseObject = false;
        public static readonly MethodInfo GenericBoxMethodInfo = GetBoxMethodInfo();

        private static readonly Dictionary<Type, Delegate> BoxingDelegates = new Dictionary<Type, Delegate>(19, InternalComparer.Type)
        {
            {typeof(bool), new BoxingDelegate<bool>(Box)},
            {typeof(byte), new BoxingDelegate<byte>(Box)},
            {typeof(sbyte), new BoxingDelegate<sbyte>(Box)},
            {typeof(ushort), new BoxingDelegate<ushort>(Box)},
            {typeof(short), new BoxingDelegate<short>(Box)},
            {typeof(uint), new BoxingDelegate<uint>(Box)},
            {typeof(int), new BoxingDelegate<int>(Box)},
            {typeof(ulong), new BoxingDelegate<ulong>(Box)},
            {typeof(long), new BoxingDelegate<long>(Box)},
            {typeof(bool?), new BoxingDelegate<bool?>(Box)},
            {typeof(byte?), new BoxingDelegate<byte?>(Box)},
            {typeof(sbyte?), new BoxingDelegate<sbyte?>(Box)},
            {typeof(ushort?), new BoxingDelegate<ushort?>(Box)},
            {typeof(short?), new BoxingDelegate<short?>(Box)},
            {typeof(uint?), new BoxingDelegate<uint?>(Box)},
            {typeof(int?), new BoxingDelegate<int?>(Box)},
            {typeof(ulong?), new BoxingDelegate<ulong?>(Box)},
            {typeof(long?), new BoxingDelegate<long?>(Box)}
        };

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
                return Cache<sbyte>.NegativeItems[~value];
            return Cache<sbyte>.Items[value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(ushort value)
        {
            if (value < CacheSize)
                return Cache<ushort>.Items[value];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(short value)
        {
            if (value < 0)
            {
                if (value >= -CacheSize)
                    return Cache<short>.NegativeItems[~value];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(int value)
        {
            if (value < 0)
            {
                if (value >= -CacheSize)
                    return Cache<int>.NegativeItems[~value];
            }
            else if (value < CacheSize)
                return Cache<int>.Items[value];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(ulong value)
        {
            if (value < CacheSize)
                return Cache<ulong>.Items[value];
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(long value)
        {
            if (value < 0)
            {
                if (value >= -CacheSize)
                    return Cache<long>.NegativeItems[~value];
            }
            else if (value < CacheSize)
                return Cache<long>.Items[value];

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(bool? value)
        {
            if (value == null)
                return null;
            if (value.Value)
                return TrueObject;
            return FalseObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(byte? value)
        {
            if (value == null)
                return null;
            return Cache<byte>.Items[value.Value];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(sbyte? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(ushort? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(short? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(uint? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(int? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(ulong? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box(long? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static object? Box<T>([AllowNull] T value)
        {
            if (BoxingTypeChecker<T>.IsBoxRequired)
                return ((BoxingDelegate<T>)BoxingDelegates[typeof(T)]).Invoke(value!);
            return value;
        }

        public static void AddBoxHandler<T>(BoxingDelegate<T> handler) where T : struct
        {
            Should.NotBeNull(handler, nameof(handler));
            BoxingDelegates[typeof(T)] = handler;
            BoxingTypeChecker<T>.IsBoxRequired = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanBox<T>()
        {
            return BoxingTypeChecker<T>.IsBoxRequired;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanBox(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            return BoxingDelegates.ContainsKey(type);
        }

        private static MethodInfo GetBoxMethodInfo()
        {
            var methods = typeof(BoxingExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (method.Name == nameof(Box) && method.IsGenericMethod)
                    return method;
            }

            Should.BeSupported(false, typeof(BoxingExtensions).Name + "." + nameof(Box));
            return null!;
        }

        #endregion

        #region Nested types

        public delegate object? BoxingDelegate<T>(T value);

        private static class BoxingTypeChecker<T>
        {
            #region Fields

            public static bool IsBoxRequired = BoxingDelegates.ContainsKey(typeof(T));

            #endregion
        }

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
                    return Default.Array<object>();

                var items = new object[cacheSize];
                if (negative)
                {
                    for (var i = -items.Length; i < 0; i++)
                        items[~i] = Convert.ChangeType(i, typeof(T));
                }
                else
                {
                    for (var i = 0; i < items.Length; i++)
                        items[i] = Convert.ChangeType(i, typeof(T));
                }

                return items;
            }

            private static int GetCacheSize(bool negative)
            {
                if (negative)
                {
                    if (typeof(T) == typeof(sbyte))
                        return -sbyte.MinValue;
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