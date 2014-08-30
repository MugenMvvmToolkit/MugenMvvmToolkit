#region Copyright
// ****************************************************************************
// <copyright file="DataConstant.cs">
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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Identifies a piece of data that could be requested from an <see cref="IDataContext" />.
    /// </summary>
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public class DataConstant : StringConstantBase<DataConstant>
    {
        #region Fields

        [IgnoreDataMember, XmlIgnore] 
        private bool _notNull;

        [IgnoreDataMember, XmlIgnore]
        private string _typeName;

        [NonSerialized, IgnoreDataMember, XmlIgnore] 
        private Type _type;

        #endregion

        #region Equality members

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public override bool Equals(DataConstant other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Id, other.Id, StringComparison.Ordinal) && Type.Equals(other.Type);
        }

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataConstant" /> class.
        /// </summary>
        protected internal DataConstant([NotNull] string id, [NotNull] Type type, bool notNull = false)
            : base(id)
        {
            Should.NotBeNull(type, "type");
            _type = type;
            _notNull = notNull;
            _typeName = type.AssemblyQualifiedName;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type of the constant.
        /// </summary>
        public Type Type
        {
            get
            {
                if (_type == null)
                    _type = Type.GetType(_typeName, true);
                return _type;
            }
        }

        /// <summary>
        ///     Gets the value that indicates that the constant value cannot be null.
        /// </summary>
        [DataMember(Name = "nn")]
        public bool NotNull
        {
            get { return _notNull; }
            internal set { _notNull = value; }
        }

        [DataMember(Name = "tn")]
        internal string TypeName
        {
            get { return _typeName; }
            set { _typeName = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Validates the specifed value.
        /// </summary>
        public virtual void Validate(object value)
        {
            if (NotNull && ReferenceEquals(value, null))
                throw ExceptionManager.DataConstantCannotBeNull(this);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        public static DataConstant Create(Expression<Func<DataConstant>> getConstant)
        {
            return CreateInternal<object>(getConstant, false);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        public static DataConstant<T> Create<T>(Expression<Func<DataConstant<T>>> getConstant)
            where T : struct
        {
            return CreateInternal<T>(getConstant, false);
        }

        /// <summary>
        ///     Creates a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        public static DataConstant<T> Create<T>(Expression<Func<DataConstant<T>>> getConstant, bool notNull)
            where T : class
        {
            return CreateInternal<T>(getConstant, notNull);
        }

        private static DataConstant<T> CreateInternal<T>(LambdaExpression getConstant, bool notNull)
        {
            Should.NotBeNull(getConstant, "getConstant");
            MemberInfo member = getConstant.GetMemberInfo();
            Type declaringType = member.DeclaringType ?? typeof (DataConstant);
            return new DataConstant<T>(declaringType.Name + "::" + member.Name, notNull);
        }

        #endregion
    }

    /// <summary>
    ///     Identifies a piece of data that could be requested from an <see cref="IDataContext" />.
    /// </summary>
    public class DataConstant<T> : IEquatable<DataConstant<T>>
    {
        #region Fields

        // ReSharper disable once StaticFieldInGenericType
        private static readonly bool IsStructType;

        #endregion

        #region Constructors

        static DataConstant()
        {
#if PCL_WINRT
            var type = typeof(T).GetTypeInfo();
#else
            var type = typeof(T);
#endif
            if (type.IsValueType)
                IsStructType = !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>);
            else
                IsStructType = false;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        public DataConstant([NotNull] string id, bool notNull = false)
            : this(id, typeof (T), notNull)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        private DataConstant([NotNull] string id, Type type, bool notNull)
            : this(new DataConstant(id, type, notNull))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataConstant{T}" /> class.
        /// </summary>
        private DataConstant(DataConstant constant)
        {
            Constant = constant;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the non-generic data constant.
        /// </summary>
        [NotNull]
        public DataConstant Constant { get; protected set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Converts the specified <see cref="DataConstant{T}" /> to <see cref="DataConstantValue" />
        /// </summary>
        public DataConstantValue ToValue(T value)
        {
            return DataConstantValue.Create(this, value);
        }

        /// <summary>
        ///     Validates the specifed value.
        /// </summary>
        public virtual void Validate(T value)
        {
            if (IsStructType || !Constant.NotNull)
                return;
            if (ReferenceEquals(value, null))
                throw ExceptionManager.DataConstantCannotBeNull(this);
        }

        public static implicit operator DataConstant<T>(string dataConstantId)
        {
            return new DataConstant<T>(dataConstantId, typeof (object), false);
        }

        public static implicit operator DataConstant(DataConstant<T> dataConstant)
        {
            return dataConstant.Constant;
        }

        public static implicit operator DataConstant<T>(DataConstant dataConstant)
        {
            return new DataConstant<T>(dataConstant);
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return Constant.ToString();
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
        public bool Equals(DataConstant<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Constant.Equals(other.Constant);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="T:System.Object" /> is equal to the current
        ///     <see cref="T:System.Object" />.
        /// </summary>
        /// <returns>
        ///     true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as DataConstant<T>;
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
            return Constant.GetHashCode();
        }

        public static bool operator ==(DataConstant<T> left, DataConstant<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DataConstant<T> left, DataConstant<T> right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}