using System;

namespace MugenMvvm.Binding.Resources
{
    public struct KnownType
    {
        #region Fields

        public readonly string Alias;
        public readonly Type Type;

        #endregion

        #region Constructors

        public KnownType(Type type, string alias)
        {
            Type = type;
            Alias = alias;
        }

        #endregion
    }
}