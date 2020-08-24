using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static class BoxingExtensions
    {
        #region Fields

        public const int CacheSize = 50;
        public static readonly object TrueObject = true;
        public static readonly object FalseObject = false;
        private static readonly MethodInfo GenericBoxMethodInfo = Initialize(out CanBoxMethodInfo);
        private static readonly MethodInfo? CanBoxMethodInfo;
        private static readonly Dictionary<Type, Func<bool>> CanBoxCache = new Dictionary<Type, Func<bool>>(InternalEqualityComparer.Type);
        private static Dictionary<Type, MethodInfo>? _boxMethods;

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
        public static object Box(byte value) => Cache<byte>.Items[value];

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
        [Preserve(Conditional = true)]
        public static object? Box<T>([AllowNull] T value)
        {
            var box = BoxingType<T>.BoxDelegate;
            if (box == null)
                return value;
            return box(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Preserve(Conditional = true)]
        public static bool CanBox<T>() => BoxingType<T>.BoxDelegate != null;

        public static bool CanBox(Type? type)
        {
            if (type == null || !type.IsValueType)
                return false;
            Func<bool>? value;
            lock (CanBoxCache)
            {
                if (!CanBoxCache.TryGetValue(type, out value))
                {
                    value = CanBoxMethodInfo!.MakeGenericMethod(type).GetMethodInvoker<Func<bool>>();
                    CanBoxCache[type] = value;
                }
            }

            return value();
        }

        public static void RegisterBoxHandler<T>(BoxingDelegate<T> handler)
        {
            Should.NotBeNull(handler, nameof(handler));
            BoxingType<T>.BoxDelegate = handler;
        }

        public static MethodInfo GetBoxMethodInfo(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (_boxMethods == null)
            {
                var boxMethods = new Dictionary<Type, MethodInfo>(19, InternalEqualityComparer.Type);
                var methods = typeof(BoxingExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name == nameof(Box) && method.IsGenericMethod)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                            boxMethods[parameters[0].ParameterType] = method;
                    }
                }

                _boxMethods = boxMethods;
            }

            if (_boxMethods.TryGetValue(type, out var m))
                return m;
            return GenericBoxMethodInfo.MakeGenericMethod(type);
        }

        private static MethodInfo Initialize(out MethodInfo canBoxMethodInfo)
        {
            RegisterBoxHandler<bool>(Box);
            RegisterBoxHandler<byte>(Box);
            RegisterBoxHandler<sbyte>(Box);
            RegisterBoxHandler<ushort>(Box);
            RegisterBoxHandler<short>(Box);
            RegisterBoxHandler<uint>(Box);
            RegisterBoxHandler<int>(Box);
            RegisterBoxHandler<ulong>(Box);
            RegisterBoxHandler<long>(Box);
            RegisterBoxHandler<bool?>(Box);
            RegisterBoxHandler<byte?>(Box);
            RegisterBoxHandler<sbyte?>(Box);
            RegisterBoxHandler<ushort?>(Box);
            RegisterBoxHandler<short?>(Box);
            RegisterBoxHandler<uint?>(Box);
            RegisterBoxHandler<int?>(Box);
            RegisterBoxHandler<ulong?>(Box);
            RegisterBoxHandler<long?>(Box);

            canBoxMethodInfo = null!;
            MethodInfo? boxMethodInfo = null;
            var methods = typeof(BoxingExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public);
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (!method.IsGenericMethod)
                    continue;

                if (method.Name == nameof(Box))
                    boxMethodInfo = method;
                else if (method.Name == nameof(CanBox))
                    canBoxMethodInfo = method;
                if (boxMethodInfo != null && canBoxMethodInfo != null)
                    return boxMethodInfo;
            }

            Should.BeSupported(false, typeof(BoxingExtensions).Name + "." + nameof(Box));
            return null!;
        }

        #endregion

        #region Nested types

        private static class BoxingType<T>
        {
            #region Fields

            public static BoxingDelegate<T>? BoxDelegate;

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

        public delegate object? BoxingDelegate<T>([AllowNull] T value);

        #endregion
    }
}