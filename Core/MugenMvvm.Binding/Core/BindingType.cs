using System;

namespace MugenMvvm.Binding.Core
{
    public struct BindingType
    {
        #region Fields

        public readonly string Alias;
        public readonly Type Type;

        #endregion

        #region Constructors

        public BindingType(Type type, string alias)
        {
            Type = type;
            Alias = alias;
        }

        #endregion
    }
}