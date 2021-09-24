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

        [StructLayout(LayoutKind.Auto)]
        public ref struct Builder<T>
        {
            public ChildBuilder<T> PinHeaderFooter(Func<T, bool?>? isHeaderOrFooter) => new(default, isHeaderOrFooter);

            public ChildBuilder<T> Ascending<TValue>(Func<T, TValue> expression) => new(SortingInfo<T>.Create(expression, true), null);

            public ChildBuilder<T> Descending<TValue>(Func<T, TValue> expression) => new(SortingInfo<T>.Create(expression, false), null);

            public ChildBuilder<T> Compare(Func<T, T, int> compare) => new(SortingInfo<T>.Create(compare), null);
        }

        public delegate ChildBuilder<T> BuilderDelegate<T>(Builder<T> builder);

        [StructLayout(LayoutKind.Auto)]
        public ref struct ChildBuilder<T>
        {
            private readonly Func<T, bool?>? _isHeaderOrFooter;
            private ItemOrListEditor<SortingInfo<T>> _sortInfo;

            internal ChildBuilder(SortingInfo<T> sortingInfo, Func<T, bool?>? isHeaderOrFooter)
            {
                _isHeaderOrFooter = isHeaderOrFooter;
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

            public SortingComparer<T> Build() => new SortingComparer<T>.Comparer(_sortInfo.ToItemOrArray(), _isHeaderOrFooter);
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
            private readonly Func<T, bool?>? _isHeaderOrFooter;
            private readonly ItemOrArray<SortingInfo<T>> _sortInfo;

            private SortingComparer(ItemOrArray<SortingInfo<T>> sortInfo, Func<T, bool?>? isHeaderOrFooter)
            {
                _sortInfo = sortInfo;
                _isHeaderOrFooter = isHeaderOrFooter;
            }

            public IComparer<object?> AsObjectComparer() => (IComparer<object?>) this;

            public int Compare(T? x, T? y)
            {
                if (_isHeaderOrFooter != null)
                {
                    var xHeaderOrFooter = _isHeaderOrFooter(x!);
                    var compare = Compare(xHeaderOrFooter, _isHeaderOrFooter(y!));
                    if (compare != 0)
                        return compare;

                    if (!xHeaderOrFooter.HasValue)
                        return 0;
                }

                foreach (var item in _sortInfo)
                {
                    var compare = item.Compare(x!, y!);
                    if (compare != 0)
                        return compare;
                }

                return 0;
            }

            private static int Compare(bool? x1, bool? x2)
            {
                if (Nullable.Equals(x1, x2))
                    return 0;

                if (x1 != null)
                {
                    if (x2 != null)
                        return x2.Value.CompareTo(x1.Value);
                    return x1.Value ? -1 : 1;
                }

                if (x2 != null)
                    return x2.Value ? 1 : -1;
                return 0;
            }

            internal sealed class Comparer : SortingComparer<T>, IComparer<object?>
            {
                internal Comparer(ItemOrArray<SortingInfo<T>> sortInfo, Func<T, bool?>? isHeaderOrFooter) : base(sortInfo, isHeaderOrFooter)
                {
                }

                int IComparer<object?>.Compare(object? x, object? y)
                {
                    if (x is T xT)
                    {
                        if (y is T yT)
                            return Compare(xT, yT);

                        if (_isHeaderOrFooter == null)
                            return -1;
                        return Compare(_isHeaderOrFooter(xT), null);
                    }

                    if (y is T yTt)
                    {
                        if (_isHeaderOrFooter == null)
                            return 1;
                        return Compare(null, _isHeaderOrFooter(yTt));
                    }

                    return 0;
                }
            }
        }
    }
}