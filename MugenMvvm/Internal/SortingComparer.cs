using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MugenMvvm.Internal
{
    public sealed class SortingComparer<T> : IComparer<T>
    {
        #region Fields

        private readonly ItemOrList<SortingInfo, List<SortingInfo>> _sortInfo;

        #endregion

        #region Constructors

        private SortingComparer(ItemOrList<SortingInfo, List<SortingInfo>> sortInfo)
        {
            _sortInfo = sortInfo;
        }

        #endregion

        #region Implementation of interfaces

        public int Compare([AllowNull] T x, [AllowNull] T y)
        {
            foreach (var item in _sortInfo)
            {
                var compare = item.Compare(x!, y!);
                if (compare == 0)
                    continue;
                return compare;
            }

            return 0;
        }

        #endregion

        #region Methods

        public static Builder Ascending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, true));

        public static Builder Descending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, false));

        #endregion

        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder
        {
            #region Fields

            private ItemOrListEditor<SortingInfo, List<SortingInfo>> _sortInfo;

            #endregion

            #region Constructors

            internal Builder(SortingInfo sortingInfo)
            {
                _sortInfo = ItemOrListEditor.Get<SortingInfo, List<SortingInfo>>(() => new List<SortingInfo>(2));
                _sortInfo.Add(sortingInfo);
            }

            #endregion

            #region Methods

            public Builder ThenBy<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo.Create(expression, true));
                return this;
            }

            public Builder ThenByDescending<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo.Create(expression, false));
                return this;
            }

            public IComparer<T> Build() => new SortingComparer<T>(_sortInfo.ToItemOrList());

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct SortingInfo
        {
            #region Fields

            private readonly Delegate _expression;
            private readonly bool _isAscending;
            private readonly Func<Delegate, bool, T, T, int> _compare;

            #endregion

            #region Constructors

            private SortingInfo(Func<Delegate, bool, T, T, int> compare, Delegate expression, bool isAscending)
            {
                _compare = compare;
                _isAscending = isAscending;
                _expression = expression;
            }

            #endregion

            #region Properties

            public bool IsEmpty => _expression == null;

            #endregion

            #region Methods

            public int Compare(T x, T y) => _compare(_expression, _isAscending, x, y);

            public static SortingInfo Create<TValue>(Func<T, TValue> expression, bool isAscending)
            {
                Should.NotBeNull(expression, nameof(expression));
                return new SortingInfo((exp, isAsc, x, y) =>
                {
                    var func = (Func<T, TValue>) exp;
                    if (isAsc)
                        return Comparer<TValue>.Default.Compare(func(x), func(y));
                    return Comparer<TValue>.Default.Compare(func(y), func(x));
                }, expression, isAscending);
            }

            #endregion
        }

        #endregion
    }
}