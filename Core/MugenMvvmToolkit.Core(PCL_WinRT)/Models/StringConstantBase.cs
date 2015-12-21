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

        //Only for serialization
        protected StringConstantBase() { }

        protected StringConstantBase(string id)
        {
            Should.NotBeNull(id, nameof(id));
            Id = id;
        }

        #endregion

        #region Properties

        [DataMember(Name = "i")]
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

        public virtual bool Equals(TType other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return _id.Equals(other._id, StringComparison.Ordinal);
        }

        public override sealed bool Equals(object obj)
        {
            var other = obj as TType;
            return other != null && Equals(other);
        }

        public override int GetHashCode()
        {
            // ReSharper disable NonReadonlyFieldInGetHashCode
            if (_hash == 0)
                _hash = _id.GetHashCode();
            return _hash;
            // ReSharper restore NonReadonlyFieldInGetHashCode
        }

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
            return Equals(left, right);
        }

        public static bool operator !=(StringConstantBase<TType> left, TType right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
