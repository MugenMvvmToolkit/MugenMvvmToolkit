using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using MugenMvvm.Constants;

namespace MugenMvvm.Internal
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        #region Fields

        public static readonly IEqualityComparer<object> Instance = new ReferenceEqualityComparer();

        #endregion

        #region Constructors

        internal ReferenceEqualityComparer()
        {
        }

        #endregion

        #region Implementation of IEqualityComparer<in object>

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            if (obj == null)
                return 0;
            return RuntimeHelpers.GetHashCode(obj) * 397;
        }

        #endregion
    }
}