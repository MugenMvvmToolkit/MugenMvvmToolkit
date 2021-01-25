using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Collections;

namespace MugenMvvm.Bindings.Core
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct BindingBuilderTo<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        private readonly BindingBuilderFrom<TTarget, TSource> _fromBuilder;
        private ItemOrListEditor<KeyValuePair<string?, object>> _parameters;
        private readonly object _pathOrExpression;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo(BindingBuilderFrom<TTarget, TSource> from, object pathOrExpression, ItemOrIReadOnlyList<KeyValuePair<string?, object>> parameters)
        {
            Should.NotBeNull(pathOrExpression, nameof(pathOrExpression));
            _fromBuilder = from;
            _pathOrExpression = pathOrExpression;
            _parameters = new ItemOrListEditor<KeyValuePair<string?, object>>(parameters, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BindingBuilderTo<TTarget, TSource> BindingParameter(string? parameterName, object value)
        {
            Should.NotBeNull(value, nameof(value));
            _parameters.Add(new KeyValuePair<string?, object>(parameterName, value));
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator BindingExpressionRequest(BindingBuilderTo<TTarget, TSource> builder) =>
            new(builder._fromBuilder.PathOrExpression, builder._pathOrExpression, builder._parameters.ToItemOrList());
    }
}