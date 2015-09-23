#region Copyright

// ****************************************************************************
// <copyright file="StringConstantBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the base class for strong type constant.
    /// </summary>
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public abstract class StringConstantBase<TType> : IEquatable<TType> where TType : StringConstantBase<TType>
    {
        #region Fields

        [NonSerialized, IgnoreDataMember, XmlIgnore]
        private int _hash;

        [IgnoreDataMember, XmlIgnore]
        private string _id;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringConstantBase{TType}" /> class.
        /// </summary>
        protected StringConstantBase(string id)
        {
            Should.NotBeNull(id, "id");
            Id = id;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the id of constant.
        /// </summary>
        [DataMember]
        public string Id
        {
            get { return _id; }
            internal set
            {
                _id = value;
                _hash = value.GetHashCode();
            }
        }

        #endregion

        #region Equality members

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public virtual bool Equals(TType other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return _id.Equals(other._id, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override sealed bool Equals(object obj)
        {
            var other = obj as TType;
            return other != null && Equals(other);
        }

        /// <summary>
        ///     Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        ///     A hash code for the current <see cref="T:System.Object" />.
        /// </returns>
        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            if (_hash == 0)
                _hash = _id.GetHashCode();
            return _hash;
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _id;
        }

        internal bool EqualsWithoutNullCheck(TType other)
        {
            return _id.Equals(other._id, StringComparison.Ordinal);
        }

        public static bool operator ==(StringConstantBase<TType> left, TType right)
        {
            return !ReferenceEquals(left, null) && left.Equals(right);
        }

        public static bool operator !=(StringConstantBase<TType> left, TType right)
        {
            return !ReferenceEquals(left, null) && !left.Equals(right);
        }

        #endregion
    }
}