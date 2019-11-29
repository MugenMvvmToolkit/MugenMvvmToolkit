using MugenMvvm.Collections;

namespace MugenMvvm.Binding.Internal
{
    internal sealed class CharLightDictionary<T> : LightDictionary<char, T>
    {
        #region Constructors

        public CharLightDictionary(int capacity) : base(capacity)
        {
        }

        #endregion

        #region Methods

        protected override bool Equals(char x, char y)
        {
            return x == y;
        }

        protected override int GetHashCode(char key)
        {
            return key.GetHashCode();
        }

        #endregion
    }
}