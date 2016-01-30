#region Copyright

// ****************************************************************************
// <copyright file="DataContext.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    public class DataContext : LightDictionaryBase<DataConstant, object>, IDataContext
    {
        #region Nested types

        private sealed class EmptyContext : IDataContext
        {
            #region Implementation of IDataContext

            public int Count => 0;

            public bool IsReadOnly => true;

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

        public static IDataContext Empty = new EmptyContext();

        #endregion

        #region Constructors

        public DataContext()
            : base(true)
        {
        }

        public DataContext(int capacity)
            : base(capacity)
        {
        }

        public DataContext([NotNull] ICollection<KeyValuePair<DataConstant, object>> values)
            : base(values.Count)
        {
            Should.NotBeNull(values, nameof(values));
            foreach (var value in values)
                Add(value.Key, value.Value);
        }

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

        public DataContext(IDataContext context)
            : base(false)
        {
            Should.NotBeNull(context, nameof(context));
            Initialize(context.Count);
            if (context.Count != 0)
                Merge(context);
        }

        #endregion

        #region Overrides of LightDictionaryBase<IDataConstant,object>

        protected override bool Equals(DataConstant x, DataConstant y)
        {
            return ReferenceEquals(x, y) || x.Equals(y);
        }

        protected override int GetHashCode(DataConstant key)
        {
            return key.GetHashCode();
        }

        #endregion

        #region Methods

        public void AddValue(DataConstant dataConstant, object value)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            dataConstant.Validate(value);
            Add(dataConstant, value);
        }

        private static T Convert<T>(object item)
        {
            if (item is T)
                return (T)item;
            return (T)ReflectionExtensions.Convert(item, typeof(T));
        }

        #endregion

        #region Implementation of IDataContext

        public bool IsReadOnly => false;

        public void Add<T>(DataConstant<T> dataConstant, T value)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            dataConstant.Validate(value);
            base.Add(dataConstant, value);
        }

        public void AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            dataConstant.Validate(value);
            base[dataConstant] = value;
        }

        public T GetData<T>(DataConstant<T> dataConstant)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            object value;
            if (!TryGetValue(dataConstant, out value))
                return default(T);
            return Convert<T>(value);
        }

        public bool TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            object value;
            if (TryGetValue(dataConstant, out value))
            {
                data = Convert<T>(value);
                return true;
            }
            data = default(T);
            return false;
        }

        public bool Contains(DataConstant dataConstant)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            return ContainsKey(dataConstant);
        }

        public new bool Remove(DataConstant dataConstant)
        {
            Should.NotBeNull(dataConstant, nameof(dataConstant));
            return base.Remove(dataConstant);
        }

        public void Merge(IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            if (ReferenceEquals(this, context))
                return;
            foreach (var item in context.ToList())
                this[item.DataConstant] = item.Value;
        }

        public new void Clear()
        {
            base.Clear();
        }

        public IList<DataConstantValue> ToList()
        {
            return this.Select(pair => DataConstantValue.Create(pair.Key, pair.Value)).ToList();
        }

        #endregion
    }
}
