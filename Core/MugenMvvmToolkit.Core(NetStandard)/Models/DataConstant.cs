#region Copyright

// ****************************************************************************
// <copyright file="DataConstant.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Attributes;

namespace MugenMvvmToolkit.Models
{
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public class DataConstant : StringConstantBase<DataConstant>
    {
        #region Fields

        [IgnoreDataMember, XmlIgnore]
        private bool _notNull;

        #endregion

        #region Constructors

        //Only for serialization
        [Preserve]
        internal DataConstant() { }

        protected internal DataConstant([NotNull] string id, bool notNull = false)
            : base(id)
        {
            NotNull = notNull;
        }

        #endregion

        #region Properties

        [DataMember(Name = "nn")]
        public bool NotNull
        {
            get { return _notNull; }
            internal set { _notNull = value; }
        }

        #endregion

        #region Methods

        public virtual void Validate(object value)
        {
            if (NotNull && ReferenceEquals(value, null))
                throw ExceptionManager.DataConstantCannotBeNull(this);
        }

        public static DataConstant Create(Type classType, string name)
        {
            return CreateInternal<object>(classType, name, false);
        }

        public static DataConstant<T> Create<T>(Type classType, string name)
            where T : struct
        {
            return CreateInternal<T>(classType, name, false);
        }

        public static DataConstant<T> Create<T>(Type classType, string name, bool notNull)
            where T : class
        {
            return CreateInternal<T>(classType, name, notNull);
        }

        public static DataConstant Create(Expression<Func<DataConstant>> getConstant)
        {
            return CreateInternal<object>(getConstant, false);
        }

        public static DataConstant<T> Create<T>(Expression<Func<DataConstant<T>>> getConstant)
            where T : struct
        {
            return CreateInternal<T>(getConstant, false);
        }

        public static DataConstant<T> Create<T>(Expression<Func<DataConstant<T>>> getConstant, bool notNull)
            where T : class
        {
            return CreateInternal<T>(getConstant, notNull);
        }

        private static DataConstant<T> CreateInternal<T>(LambdaExpression getConstant, bool notNull)
        {
            Should.NotBeNull(getConstant, nameof(getConstant));
            MemberInfo member = getConstant.GetMemberInfo();
            return CreateInternal<T>(member.DeclaringType ?? typeof(DataConstant), member.Name, notNull);
        }

        private static DataConstant<T> CreateInternal<T>(Type classType, string name, bool notNull)
        {
            Should.NotBeNull(classType, nameof(classType));
            return new DataConstant<T>(classType.Name + "_" + classType.FullName.Length.ToString() + "::" + name, notNull);
        }

        #endregion
    }

    public class DataConstant<T> : IEquatable<DataConstant<T>>
    {
        #region Fields

        // ReSharper disable once StaticFieldInGenericType
        private static readonly bool IsStructType;

        #endregion

        #region Constructors

        static DataConstant()
        {
#if NET_STANDARD
            var type = typeof(T).GetTypeInfo();
#else
            var type = typeof(T);
#endif
            if (type.IsValueType)
                IsStructType = !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>);
            else
                IsStructType = false;
        }

        public DataConstant([NotNull] string id, bool notNull = false)
            : this(new DataConstant(id, notNull))
        {
        }

        public DataConstant(DataConstant constant)
        {
            Constant = constant;
        }

        #endregion

        #region Properties

        [NotNull]
        public DataConstant Constant { get; protected set; }

        #endregion

        #region Methods

        public DataConstantValue ToValue(T value)
        {
            return DataConstantValue.Create(this, value);
        }

        public virtual void Validate(T value)
        {
            if (IsStructType || !Constant.NotNull)
                return;
            if (ReferenceEquals(value, null))
                throw ExceptionManager.DataConstantCannotBeNull(this);
        }

        public static implicit operator DataConstant<T>(string dataConstantId)
        {
            return new DataConstant<T>(dataConstantId, false);
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

        public override string ToString()
        {
            return Constant.ToString();
        }

        #endregion

        #region Equality members

        public bool Equals(DataConstant<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Constant.Equals(other.Constant);
        }

        public sealed override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as DataConstant<T>;
            return other != null && Equals(other);
        }

        public sealed override int GetHashCode()
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
