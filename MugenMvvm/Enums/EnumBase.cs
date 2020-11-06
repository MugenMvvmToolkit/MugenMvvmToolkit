using System;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Enums
{
    public static class EnumBase
    {
        #region Methods

        public static TEnum[] GetAll<TEnum>() where TEnum : class, IEnum
        {
            var provider = EnumProvider<TEnum>.Provider;
            if (provider == null)
                return Default.Array<TEnum>();
            return provider();
        }

        public static void SetEnumProvider<TEnum>(Func<TEnum[]> provider) where TEnum : class, IEnum
        {
            Should.NotBeNull(provider, nameof(provider));
            EnumProvider<TEnum>.Provider = provider;
        }

        #endregion

        #region Nested types

        private static class EnumProvider<TEnum> where TEnum : class, IEnum
        {
            #region Fields

            public static Func<TEnum[]>? Provider;

            #endregion
        }

        #endregion
    }
}