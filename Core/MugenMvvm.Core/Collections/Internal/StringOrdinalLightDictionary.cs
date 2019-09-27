namespace MugenMvvm.Collections.Internal
{
    internal sealed class StringOrdinalLightDictionary<TValue> : LightDictionary<string, TValue>
    {
        #region Constructors

        public StringOrdinalLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(string x, string y)
        {
            return x.Equals(y);
        }

        protected override int GetHashCode(string key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}