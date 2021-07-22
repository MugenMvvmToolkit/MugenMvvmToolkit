using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;

namespace MugenMvvm.Internal
{
    public abstract class SortingComparer<T> : IComparer<T>
    {
        private readonly ItemOrArray<SortingInfo> _sortInfo;

        private SortingComparer(ItemOrArray<SortingInfo> sortInfo)
        {
            _sortInfo = sortInfo;
        }

        public static Builder Ascending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, true));

        public static Builder Descending<TValue>(Func<T, TValue> expression) => new(SortingInfo.Create(expression, false));

        public static Builder Compare(Func<T, T, int> compare) => new(SortingInfo.Create(compare));

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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Builder(SortingInfo sortingInfo)
            {
                _sortInfo = new ItemOrListEditor<SortingInfo>(2);
                _sortInfo.Add(sortingInfo);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder ThenBy<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo.Create(expression, true));
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder ThenByDescending<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo.Create(expression, false));
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Builder ThenCompare(Func<T, T, int> compare)
            {
                _sortInfo.Add(SortingInfo.Create(compare));
                return this;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Comparer Build() => new(_sortInfo.ToItemOrArray());
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

            [MemberNotNullWhen(false, nameof(_expression))]
            public bool IsEmpty => _expression == null;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

            public static SortingInfo Create(Func<T, T, int> compare)
            {
                Should.NotBeNull(compare, nameof(compare));
                return new SortingInfo((exp, _, x, y) => ((Func<T, T, int>) exp).Invoke(x, y), compare, false);
            }
        }

        public sealed class Comparer : SortingComparer<T>, IComparer<object?>
        {
            internal Comparer(ItemOrArray<SortingInfo> sortInfo) : base(sortInfo)
            {
            }

            int IComparer<object?>.Compare(object? x, object? y) => Compare((T) x!, (T) y!);
        }
    }
}