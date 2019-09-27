using System;

namespace MugenMvvm.Collections.Internal
{
    internal sealed class TypeLightDictionary<TValue> : LightDictionary<Type, TValue>
    {
        #region Constructors

        public TypeLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(Type x, Type y)
        {
            return x.EqualsEx(y);
        }

        protected override int GetHashCode(Type key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}