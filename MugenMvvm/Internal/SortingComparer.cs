using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;

namespace MugenMvvm.Internal
{
    public sealed class SortingComparer<T> : IComparer<T>
    {
        private readonly ItemOrIReadOnlyList<SortingInfo> _sortInfo;

        private SortingComparer(ItemOrIReadOnlyList<SortingInfo> sortInfo)
        {
            _sortInfo = sortInfo;
        }

        public static Builder Ascending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, true));

        public static Builder Descending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, false));

        public int Compare(T? x, T? y)
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

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder
        {
            private ItemOrListEditor<SortingInfo> _sortInfo;

            internal Builder(SortingInfo sortingInfo)
            {
                _sortInfo = new ItemOrListEditor<SortingInfo>();
                _sortInfo.Add(sortingInfo);
            }

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
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct SortingInfo
        {
            private readonly Delegate _expression;
            private readonly bool _isAscending;
            private readonly Func<Delegate, bool, T, T, int> _compare;

            private SortingInfo(Func<Delegate, bool, T, T, int> compare, Delegate expression, bool isAscending)
            {
                _compare = compare;
                _isAscending = isAscending;
                _expression = expression;
            }

            public bool IsEmpty => _expression == null;

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
        }
    }
}