using System;
using System.Collections.Generic;
using System.Reflection;

namespace MugenMvvm.Infrastructure.Internal
{
    public sealed class MemberInfoComparer : IEqualityComparer<MemberInfo>, IEqualityComparer<Type>
    {
        #region Fields

        public static readonly MemberInfoComparer Instance;

        #endregion

        #region Constructors

        static MemberInfoComparer()
        {
            Instance = new MemberInfoComparer();
        }

        private MemberInfoComparer()
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool Equals(MemberInfo x, MemberInfo y)
        {
            return x.EqualsEx(y);
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }

        public bool Equals(Type x, Type y)
        {
            return x.EqualsEx(y);
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }

        #endregion
    }
}