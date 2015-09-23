#region Copyright

// ****************************************************************************
// <copyright file="DataContext.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the specific operation context.
    /// </summary>
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public class DataContext : LightDictionaryBase<DataConstant, object>, IDataContext
    {
        #region Nested types

        private sealed class EmptyContext : IDataContext
        {
            #region Implementation of IDataContext

            public int Count
            {
                get { return 0; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void Add<T>(DataConstant<T> dataConstant, T value)
            {
            }

            public void AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
            {
            }

            public T GetData<T>(DataConstant<T> dataConstant)
            {
                return default(T);
            }

            public bool TryGetData<T>(DataConstant<T> dataConstant, out T data)
            {
                data = default(T);
                return false;
            }

            public bool Contains(DataConstant dataConstant)
            {
                return false;
            }

            public bool Remove(DataConstant dataConstant)
            {
                return false;
            }

            public void Merge(IDataContext context)
            {
            }

            public void Clear()
            {
            }

            public IList<DataConstantValue> ToList()
            {
                return MugenMvvmToolkit.Empty.Array<DataConstantValue>();
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Gets the empty data context.
        /// </summary>
        public static IDataContext Empty = new EmptyContext();

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContext" /> class.
        /// </summary>
        public DataContext()
            : base(true)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContext" /> class.
        /// </summary>
        public DataContext(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContext" /> class.
        /// </summary>
        public DataContext([NotNull] ICollection<KeyValuePair<DataConstant, object>> values)
            : base(values.Count)
        {
            Should.NotBeNull(values, "values");
            foreach (var value in values)
                Add(value.Key, value.Value);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContext" /> class.
        /// </summary>
        public DataContext(params DataConstantValue[] array)
            : base(false)
        {
            if (array != null && array.Length != 0)
            {
                Initialize(array.Length);
                for (int i = 0; i < array.Length; i++)
                {
                    DataConstantValue value = array[i];
                    if (!value.IsEmpty)
                        Add(value.DataConstant, value.Value);
                }
            }
            else
                Initialize(0);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataContext" /> class.
        /// </summary>
        public DataContext(IDataContext context)
            : base(false)
        {
            Should.NotBeNull(context, "context");
            Initialize(context.Count);
            if (context.Count != 0)
                Merge(context);
        }

        #endregion

        #region Overrides of LightDictionaryBase<IDataConstant,object>

        /// <summary>
        ///     Determines whether the specified objects are equal.
        /// </summary>
        protected override bool Equals(DataConstant x, DataConstant y)
        {
            return ReferenceEquals(x, y) || x.EqualsWithoutNullCheck(y);
        }

        /// <summary>
        ///     Returns a hash code for the specified object.
        /// </summary>
        protected override int GetHashCode(DataConstant key)
        {
            return key.GetHashCode();
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        public void AddValue(DataConstant dataConstant, object value)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            dataConstant.Validate(value);
            Add(dataConstant, value);
        }

        #endregion

        #region Implementation of IDataContext

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IDataContext" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="IDataContext" /> is read-only; otherwise, false.
        /// </returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        public void Add<T>(DataConstant<T> dataConstant, T value)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            dataConstant.Validate(value);
            base.Add(dataConstant, value);
        }

        /// <summary>
        ///     Adds the data constant value or update existing.
        /// </summary>
        public void AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            dataConstant.Validate(value);
            base[dataConstant] = value;
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        public T GetData<T>(DataConstant<T> dataConstant)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            object value;
            if (!TryGetValue(dataConstant, out value))
                return default(T);
            return (T)value;
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        public bool TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            object value;
            if (TryGetValue(dataConstant, out value))
            {
                data = (T)value;
                return true;
            }
            data = default(T);
            return false;
        }

        /// <summary>
        ///     Determines whether the <see cref="IDataContext" /> contains the specified key.
        /// </summary>
        public bool Contains(DataConstant dataConstant)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            return ContainsKey(dataConstant);
        }

        /// <summary>
        ///     Removes the data constant value.
        /// </summary>
        public new bool Remove(DataConstant dataConstant)
        {
            Should.NotBeNull(dataConstant, "dataConstant");
            return base.Remove(dataConstant);
        }

        /// <summary>
        ///     Updates the current context.
        /// </summary>
        public void Merge(IDataContext context)
        {
            Should.NotBeNull(context, "context");
            if (ReferenceEquals(this, context))
                return;
            foreach (var item in context.ToList())
                this[item.DataConstant] = item.Value;
        }

        /// <summary>
        /// Removes all values from current context.
        /// </summary>
        public new void Clear()
        {
            base.Clear();
        }

        /// <summary>
        ///     Creates an instance of <see cref="IList{DataConstantValue}" /> from current context.
        /// </summary>
        public IList<DataConstantValue> ToList()
        {
            return this.Select(pair => DataConstantValue.Create(pair.Key, pair.Value)).ToList();
        }

        #endregion
    }
}