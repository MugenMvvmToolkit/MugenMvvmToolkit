using System.Reflection;

namespace MugenMvvm.Collections.Internal
{
    internal sealed class MemberInfoLightDictionary<TKey, TValue> : LightDictionary<TKey, TValue>
        where TKey : MemberInfo
    {
        #region Constructors

        public MemberInfoLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(TKey x, TKey y)
        {
            return x.EqualsEx(y);
        }

        protected override int GetHashCode(TKey key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}