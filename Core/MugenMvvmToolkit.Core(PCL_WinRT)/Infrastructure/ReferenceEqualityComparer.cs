#region Copyright
// ****************************************************************************
// <copyright file="ReferenceEqualityComparer.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the reference equality comparer
    /// </summary>
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    public class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        #region Fields

        /// <summary>
        ///     Gets an instance of <see cref="ReferenceEqualityComparer" />.
        /// </summary>
        public static readonly IEqualityComparer<object> Instance;

        #endregion

        #region Constructors

        static ReferenceEqualityComparer()
        {
            Instance = new ReferenceEqualityComparer();
        }

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
                return 1;
            return RuntimeHelpers.GetHashCode(obj);
        }

        #endregion
    }
}