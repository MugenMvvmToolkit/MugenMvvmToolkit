using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Enums
{
    public static class EnumBase
    {
        #region Fields

        private static readonly Dictionary<Type, Func<IEnum[]>> TypeToEnums = new Dictionary<Type, Func<IEnum[]>>(InternalEqualityComparer.Type);
        private static readonly Dictionary<Type, Func<string?, IEnum?, bool, IEnum?>> TypeToNameResolver = new Dictionary<Type, Func<string?, IEnum?, bool, IEnum?>>(InternalEqualityComparer.Type);

        #endregion

        #region Methods

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
            => EnumProvider<TEnum>.NameResolver?.Invoke(value, defaultValue, ignoreCase);

        public static IEnum[] GetAll(Type enumType)
        {
            Should.BeOfType<IEnum>(enumType, nameof(enumType));
            if (TypeToEnums.TryGetValue(enumType, out var provider))
                return provider();
            return Default.Array<IEnum>();
        }

        public static TEnum[] GetAll<TEnum>() where TEnum : class, IEnum
        {
            var provider = EnumProvider<TEnum>.Provider;
            if (provider == null)
                return Default.Array<TEnum>();
            return provider();
        }

        public static void SetEnumProvider<TEnum>(Func<TEnum[]> provider, Func<string?, TEnum?, bool, TEnum?> nameResolver) where TEnum : class, IEnum
        {
            Should.NotBeNull(provider, nameof(provider));
            Should.NotBeNull(nameResolver, nameof(nameResolver));
            EnumProvider<TEnum>.Provider = provider;
            EnumProvider<TEnum>.NameResolver = nameResolver;
            TypeToEnums[typeof(TEnum)] = provider;
            TypeToNameResolver[typeof(TEnum)] = nameResolver.TryGetByName;
        }

        private static IEnum? TryGetByName<TEnum>(this Func<string?, TEnum?, bool, TEnum?> resolver, string? name, IEnum? defaultValue, bool ignoreCase) where TEnum : class, IEnum
            => resolver(name, (TEnum?) defaultValue, ignoreCase);

        #endregion

        #region Nested types

        private static class EnumProvider<TEnum> where TEnum : class, IEnum
        {
            #region Fields

            public static Func<TEnum[]>? Provider;
            public static Func<string?, TEnum?, bool, TEnum?>? NameResolver;

            #endregion
        }

        #endregion
    }
}