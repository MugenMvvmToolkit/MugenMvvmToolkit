using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Internal
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    public sealed class HasPriorityComparer : IComparer<IHasPriority>
    {
        #region Fields

        public static readonly HasPriorityComparer Instance = new HasPriorityComparer();

        #endregion

        #region Constructors

        private HasPriorityComparer()
        {
        }

        #endregion

        #region Implementation of interfaces

        public int Compare(IHasPriority x, IHasPriority y)
        {
            var x1 = y?.Priority ?? int.MinValue;
            var x2 = x?.Priority ?? int.MinValue;
            return x1.CompareTo(x2);
        }

        #endregion
    }
}