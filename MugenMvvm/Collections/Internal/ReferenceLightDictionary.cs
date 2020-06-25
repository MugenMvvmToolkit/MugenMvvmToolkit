using System.Runtime.CompilerServices;

namespace MugenMvvm.Collections.Internal
{
    internal sealed class ReferenceLightDictionary<TValue> : LightDictionary<object, TValue>
    {
        #region Constructors

        public ReferenceLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        protected override int GetHashCode(object key)
        {
            return RuntimeHelpers.GetHashCode(key);
        }

        #endregion
    }
}