using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Android.Internal;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Android.Bindings
{
    public sealed class NativeStringBindingExpressionCache : ComponentCacheBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent,
        IEqualityComparer<object>
    {
        private readonly Dictionary<object, object?> _cache;

        public NativeStringBindingExpressionCache(int priority = BindingComponentPriority.BuilderCache)
            : base(priority)
        {
            _cache = new Dictionary<object, object?>(59, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ComputeHash32(ReadOnlySpan<char> value)
        {
            unchecked
            {
                var hash1 = (5381 << 16) + 5381;
                var hash2 = hash1;

                for (var i = 0; i < value.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ value[i];
                    if (i == value.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ value[i + 1];
                }

                return hash1 + hash2 * 1566083941;
            }
        }

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is not NativeStringAccessor s)
                return Components.TryParseBindingExpression(bindingManager, expression, metadata);

            if (!_cache.TryGetValue(s, out var value))
            {
                value = Components.TryParseBindingExpression(bindingManager, expression, metadata).GetRawValue();
                _cache[s.Span.ToArray()] = value;
            }

            return ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(value);
        }

        protected override void Invalidate(object? state, IReadOnlyMetadataContext? metadata) => _cache.Clear();

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            if (x is char[] s)
                return ((NativeStringAccessor)y).Span.Equals(s, StringComparison.Ordinal);
            return ((NativeStringAccessor)x).Span.Equals((char[])y, StringComparison.Ordinal);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            //todo replace to string.GetHashCode(ReadOnlySpan) when xamarin supports it
            if (obj is char[] s)
                return ComputeHash32(s);
            return ComputeHash32(((NativeStringAccessor)obj).Span);
        }
    }
}