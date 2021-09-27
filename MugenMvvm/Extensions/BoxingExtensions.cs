using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Delegates;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static class BoxingExtensions
    {
        public const int CacheSize = 50;
        public static readonly object TrueObject = true;
        public static readonly object FalseObject = false;
        private static readonly MethodInfo GenericBoxMethodInfo = Initialize(out CanBoxMethodInfo);
        private static readonly MethodInfo? CanBoxMethodInfo;
        private static readonly Dictionary<Type, Func<bool>> CanBoxCache = new(InternalEqualityComparer.Type);
        private static Dictionary<Type, MethodInfo>? _boxMethods;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(bool value)
        {
            if (value)
                return TrueObject;
            return FalseObject;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object Box(int value)
        {
            if (value < 0)
            {
                if (value >= -CacheSize)
                    return IntCache.Negative[~value];
            }
            else if (value < CacheSize)
                return IntCache.Positive[value];

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
        public static object? Box(int? value)
        {
            if (value == null)
                return null;
            return Box(value.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        [Preserve(Conditional = true)]
        public static object? Box<T>(T? value)
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
                    value = (Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>), null, CanBoxMethodInfo!.MakeGenericMethod(type));
                    CanBoxCache[type] = value;
                }
            }

            return value();
        }

        public static void RegisterBoxHandler<T>(BoxingDelegate<T> handler, MethodInfo? methodInfo = null)
        {
            Should.NotBeNull(handler, nameof(handler));
            BoxingType<T>.BoxDelegate = handler;
            if (methodInfo != null)
                GetBoxMethods()[typeof(T)] = methodInfo;
        }

        public static MethodInfo GetBoxMethodInfo(Type type)
        {
            Should.NotBeNull(type, nameof(type));
            if (GetBoxMethods().TryGetValue(type, out var m))
                return m;
            return GenericBoxMethodInfo.MakeGenericMethod(type);
        }

        private static MethodInfo Initialize(out MethodInfo canBoxMethodInfo)
        {
            RegisterBoxHandler<bool>(Box);
            RegisterBoxHandler<int>(Box);
            RegisterBoxHandler<bool?>(Box);
            RegisterBoxHandler<int?>(Box);

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

        private static Dictionary<Type, MethodInfo> GetBoxMethods()
        {
            if (_boxMethods == null)
            {
                var boxMethods = new Dictionary<Type, MethodInfo>(19, InternalEqualityComparer.Type);
                var methods = typeof(BoxingExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name == nameof(Box) && !method.IsGenericMethod)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1)
                            boxMethods[parameters[0].ParameterType] = method;
                    }
                }

                _boxMethods = boxMethods;
            }

            return _boxMethods;
        }

        

        internal static class IntCache
        {
            public static readonly object[] Positive = GenerateItems(false);
            public static readonly object[] Negative = GenerateItems(true);

            private static object[] GenerateItems(bool negative)
            {
                var items = new object[CacheSize];
                if (negative)
                {
                    for (var i = -items.Length; i < 0; i++)
                        items[~i] = i;
                }
                else
                {
                    for (var i = 0; i < items.Length; i++)
                        items[i] = i;
                }

                return items;
            }
        }

        private static class BoxingType<T>
        {
            public static BoxingDelegate<T>? BoxDelegate;
        }
    }
}