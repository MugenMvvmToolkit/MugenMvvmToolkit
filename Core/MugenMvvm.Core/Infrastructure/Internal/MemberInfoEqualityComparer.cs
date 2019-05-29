using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using MugenMvvm.Constants;

namespace MugenMvvm.Infrastructure.Internal
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public sealed class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>, IEqualityComparer<Type>
    {
        #region Fields

        public static readonly MemberInfoEqualityComparer Instance = new MemberInfoEqualityComparer();

        #endregion

        #region Constructors

        private MemberInfoEqualityComparer()
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