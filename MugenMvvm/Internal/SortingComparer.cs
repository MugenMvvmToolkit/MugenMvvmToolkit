using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Collections;

namespace MugenMvvm.Internal
{
    public static class SortingComparerBuilder
    {
        public static Builder<T> Get<T>() => default;

        public delegate ChildBuilder<T> BuilderDelegate<T>(Builder<T> builder);

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T>
        {
            public ChildBuilder<T> Ascending<TValue>(Func<T, TValue> expression) => new(SortingInfo<T>.Create(expression, true));

            public ChildBuilder<T> Descending<TValue>(Func<T, TValue> expression) => new(SortingInfo<T>.Create(expression, false));

            public ChildBuilder<T> Compare(Func<T, T, int> compare) => new(SortingInfo<T>.Create(compare));

            public ChildBuilder<T> Compare<TValue>(Func<T, TValue> expression, bool isAscending) => new(SortingInfo<T>.Create(expression, isAscending));
        }

        [StructLayout(LayoutKind.Auto)]
        public ref struct ChildBuilder<T>
        {
            private ItemOrListEditor<SortingInfo<T>> _sortInfo;

            internal ChildBuilder(SortingInfo<T> sortingInfo)
            {
                _sortInfo = new ItemOrListEditor<SortingInfo<T>>(2);
                if (!sortingInfo.IsEmpty)
                    _sortInfo.Add(sortingInfo);
            }

            public ChildBuilder<T> ThenBy<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo<T>.Create(expression, true));
                return this;
            }

            public ChildBuilder<T> ThenByDescending<TValue>(Func<T, TValue> expression)
            {
                _sortInfo.Add(SortingInfo<T>.Create(expression, false));
                return this;
            }

            public ChildBuilder<T> ThenCompare(Func<T, T, int> compare)
            {
                _sortInfo.Add(SortingInfo<T>.Create(compare));
                return this;
            }

            public ChildBuilder<T> ThenCompare<TValue>(Func<T, TValue> expression, bool isAscending)
            {
                _sortInfo.Add(SortingInfo<T>.Create(expression, isAscending));
                return this;
            }

            public SortingComparer<T> Build() => new SortingComparer<T>.Comparer(_sortInfo.ToItemOrArray());
        }

        [StructLayout(LayoutKind.Auto)]
        internal readonly struct SortingInfo<T>
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

            public static SortingInfo<T> Create<TValue>(Func<T, TValue> expression, bool isAscending)
            {
                Should.NotBeNull(expression, nameof(expression));
                return new SortingInfo<T>((exp, isAsc, x, y) =>
                {
                    var func = (Func<T, TValue>) exp;
                    if (isAsc)
                        return Comparer<TValue>.Default.Compare(func(x), func(y));
                    return Comparer<TValue>.Default.Compare(func(y), func(x));
                }, expression, isAscending);
            }

            public static SortingInfo<T> Create(Func<T, T, int> compare)
            {
                Should.NotBeNull(compare, nameof(compare));
                return new SortingInfo<T>((exp, _, x, y) => ((Func<T, T, int>) exp).Invoke(x, y), compare, false);
            }
        }

        public abstract class SortingComparer<T> : IComparer<T>
        {
            private readonly ItemOrArray<SortingInfo<T>> _sortInfo;

            private SortingComparer(ItemOrArray<SortingInfo<T>> sortInfo)
            {
                _sortInfo = sortInfo;
            }

            public IComparer<object?> AsObjectComparer() => (IComparer<object?>) this;

            public int Compare(T? x, T? y)
            {
                foreach (var item in _sortInfo)
                {
                    var compare = item.Compare(x!, y!);
                    if (compare != 0)
                        return compare;
                }

                return 0;
            }

            internal sealed class Comparer : SortingComparer<T>, IComparer<object?>
            {
                internal Comparer(ItemOrArray<SortingInfo<T>> sortInfo) : base(sortInfo)
                {
                }

                int IComparer<object?>.Compare(object? x, object? y)
                {
                    if (x is T xT)
                    {
                        if (y is T yT)
                            return Compare(xT, yT);

                        return -1;
                    }

                    if (y is T)
                        return 1;
                    return 0;
                }
            }
        }
    }
}