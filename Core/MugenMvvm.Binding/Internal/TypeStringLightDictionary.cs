using System;
using MugenMvvm.Collections;

namespace MugenMvvm.Binding.Internal
{
    internal sealed class TypeStringLightDictionary<TValue> : LightDictionary<TypeStringKey, TValue>
    {
        #region Constructors

        public TypeStringLightDictionary(int capacity)
            : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(TypeStringKey x, TypeStringKey y)
        {
            return x.Type == y.Type && string.Equals(x.Name, y.Name);
        }

        protected override int GetHashCode(TypeStringKey key)
        {
            return HashCode.Combine(key.Type, key.Name);
        }

        #endregion
    }
}