using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Enums
{
    public static class EnumBase
    {
        private static readonly Dictionary<Type, Func<IEnum[]>> TypeToEnums = new(InternalEqualityComparer.Type);
        private static readonly Dictionary<Type, Func<string?, IEnum?, bool, IEnum?>> TypeToNameResolver = new(InternalEqualityComparer.Type);
        private static readonly Dictionary<Type, Delegate> TypeToValueResolver = new(InternalEqualityComparer.Type);

        public static bool ThrowOnDuplicate { get; set; } = true;

        [return: NotNullIfNotNull("defaultValue")]
        public static IEnum? TryGet<TValue>(Type enumType, TValue value, IEnum? defaultValue = null)
            where TValue : IComparable<TValue>, IEquatable<TValue>
        {
            Should.BeOfType<IEnum>(enumType, nameof(enumType));
            if (TypeToValueResolver.TryGetValue(enumType, out var del) && del is Func<TValue, IEnum?, IEnum?> provider)
                return provider(value, defaultValue);
            return defaultValue;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static TEnum? TryGet<TEnum, TValue>(TValue value, TEnum? defaultValue = null)
            where TEnum : EnumBase<TEnum, TValue>
            where TValue : IComparable<TValue>, IEquatable<TValue> =>
            EnumProvider<TEnum, TValue>.ValueResolver?.Invoke(value, defaultValue) ?? defaultValue;

        [return: NotNullIfNotNull("defaultValue")]
        public static IEnum? TryGetByName(Type enumType, string? value, IEnum? defaultValue = null, bool ignoreCase = false)
        {
            Should.BeOfType<IEnum>(enumType, nameof(enumType));
            if (TypeToNameResolver.TryGetValue(enumType, out var resolver))
                return resolver(value, defaultValue, ignoreCase);
            return defaultValue;
        }

        [return: NotNullIfNotNull("defaultValue")]
        public static TEnum? TryGetByName<TEnum>(string? value, TEnum? defaultValue = null, bool ignoreCase = false) where TEnum : class, IEnum
            => EnumProvider<TEnum>.NameResolver?.Invoke(value, defaultValue, ignoreCase) ?? defaultValue;

        public static IEnum[] GetAll(Type enumType)
        {
            Should.BeOfType<IEnum>(enumType, nameof(enumType));
            if (TypeToEnums.TryGetValue(enumType, out var provider))
                return provider();
            return Array.Empty<IEnum>();
        }

        public static TEnum[] GetAll<TEnum>() where TEnum : class, IEnum
        {
            var provider = EnumProvider<TEnum>.Provider;
            if (provider == null)
                return Array.Empty<TEnum>();
            return provider();
        }

        public static void SetEnumProvider<TEnum, TValue>(Func<TEnum[]> provider, Func<TValue, TEnum?, TEnum?> valueResolver, Func<string?, TEnum?, bool, TEnum?> nameResolver)
            where TEnum : EnumBase<TEnum, TValue>
            where TValue : IComparable<TValue>, IEquatable<TValue>
        {
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(nameResolver, nameof(nameResolver));
            EnumProvider<TEnum>.Provider = provider;
            EnumProvider<TEnum>.NameResolver = nameResolver;
            EnumProvider<TEnum, TValue>.ValueResolver = valueResolver;
            TypeToEnums[typeof(TEnum)] = provider;
            TypeToNameResolver[typeof(TEnum)] = nameResolver.TryGetByName;
            TypeToValueResolver[typeof(TEnum)] = new Func<TValue, IEnum?, IEnum?>(valueResolver.TryGetByValue);
        }

        private static IEnum? TryGetByName<TEnum>(this Func<string?, TEnum?, bool, TEnum?> resolver, string? name, IEnum? defaultValue, bool ignoreCase) where TEnum : class, IEnum
            => resolver(name, (TEnum?) defaultValue, ignoreCase);

        private static IEnum? TryGetByValue<TEnum, TValue>(this Func<TValue, TEnum?, TEnum?> resolver, TValue value, IEnum? defaultValue) where TEnum : class, IEnum
            => resolver(value, (TEnum?) defaultValue);

        private static class EnumProvider<TEnum> where TEnum : class, IEnum
        {
            public static Func<TEnum[]>? Provider;
            public static Func<string?, TEnum?, bool, TEnum?>? NameResolver;
        }

        private static class EnumProvider<TEnum, TValue> where TEnum : class, IEnum
        {
            public static Func<TValue, TEnum?, TEnum?>? ValueResolver;
        }
    }
}