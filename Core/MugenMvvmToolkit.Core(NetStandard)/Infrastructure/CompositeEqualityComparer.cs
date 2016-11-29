#region Copyright

// ****************************************************************************
// <copyright file="CompositeEqualityComparer.cs">
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;

namespace MugenMvvmToolkit.Infrastructure
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace), Serializable]
    public class CompositeEqualityComparer : IEqualityComparer<object>, IEqualityComparer
    {
        #region Nested types

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace), Serializable]
        internal sealed class ComparerWrapper<T> : IComparerWrapper
        {
            #region Fields

            [DataMember]
            internal readonly IEqualityComparer<T> Comparer;

            [DataMember]
            internal readonly bool ExactlySameType;

            [NonSerialized, IgnoreDataMember]
            private readonly Func<T, T, bool> _equalsDel;

            [NonSerialized, IgnoreDataMember]
            private readonly Func<T, int> _getHashDel;

            #endregion

            #region Constructors

            //Only for serialization
            [Preserve(Conditional = true)]
            internal ComparerWrapper() { }

            public ComparerWrapper([NotNull] IEqualityComparer<T> comparer, bool exactlySameType)
            {
                Should.NotBeNull(comparer, nameof(comparer));
                Comparer = comparer;
                ExactlySameType = exactlySameType;
            }

            public ComparerWrapper(Func<T, T, bool> equalsDel, Func<T, int> getHashDel, bool exactlySameType)
            {
                Should.NotBeNull(equalsDel, nameof(equalsDel));
                Should.NotBeNull(getHashDel, nameof(getHashDel));
                _equalsDel = equalsDel;
                _getHashDel = getHashDel;
                ExactlySameType = exactlySameType;
            }

            #endregion

            #region Implementation of IEqualityComparer

            public new bool Equals(object x, object y)
            {
                var valueX = (T)x;
                var valueY = (T)y;
                if (_equalsDel != null)
                    return _equalsDel(valueX, valueY);
                if (Comparer != null)
                    return Comparer.Equals(valueX, valueY);
                throw new NotSupportedException();
            }

            public int GetHashCode(object obj)
            {
                var value = (T)obj;
                if (_getHashDel != null)
                    return _getHashDel(value);
                if (Comparer != null)
                    return Comparer.GetHashCode(value);
                throw new NotSupportedException();
            }

            public bool IsCompatible(object item)
            {
                if (ExactlySameType)
                {
                    if (item == null)
                        return false;
                    return item.GetType() == typeof(T);
                }
                return item is T;
            }

            #endregion
        }

        private interface IComparerWrapper : IEqualityComparer
        {
            bool IsCompatible(object item);
        }

        #endregion

        #region Fields

        private readonly object _locker;
        private IComparerWrapper[] _comparers;

        #endregion

        #region Constructors

        public CompositeEqualityComparer()
        {
            _locker = new object();
            _comparers = Empty.Array<IComparerWrapper>();
        }

        #endregion

        #region Methods

        public CompositeEqualityComparer AddComparer<T>([NotNull] IEqualityComparer<T> comparer, bool exactlySameType = false)
        {
            var comparerWrapper = new ComparerWrapper<T>(comparer, exactlySameType);
            lock (_locker)
            {
                Array.Resize(ref _comparers, _comparers.Length + 1);
                _comparers[_comparers.Length - 1] = comparerWrapper;
            }
            return this;
        }

        public CompositeEqualityComparer AddComparer<T>([NotNull] Func<T, T, bool> equalsDel, [NotNull] Func<T, int> getHashDel, bool exactlySameType = false)
        {
            var comparerWrapper = new ComparerWrapper<T>(equalsDel, getHashDel, exactlySameType);
            lock (_locker)
            {
                Array.Resize(ref _comparers, _comparers.Length + 1);
                _comparers[_comparers.Length - 1] = comparerWrapper;
            }
            return this;
        }

        #endregion

        #region Implementation of IEqualityComparer<in object>

        public int GetHashCode(object obj)
        {
            IComparerWrapper[] comparers = _comparers;
            for (int i = 0; i < comparers.Length; i++)
            {
                IComparerWrapper equalityComparer = comparers[i];
                if (equalityComparer.IsCompatible(obj))
                    return equalityComparer.GetHashCode(obj);
            }
            return ReferenceEqualityComparer.Instance.GetHashCode(obj);
        }

        public new bool Equals(object x, object y)
        {
            IComparerWrapper[] comparers = _comparers;
            for (int i = 0; i < comparers.Length; i++)
            {
                IComparerWrapper equalityComparer = comparers[i];
                if (equalityComparer.IsCompatible(x) && equalityComparer.IsCompatible(y))
                    return equalityComparer.Equals(x, y);
            }
            return ReferenceEqualityComparer.Instance.Equals(x, y);
        }

        #endregion
    }
}
