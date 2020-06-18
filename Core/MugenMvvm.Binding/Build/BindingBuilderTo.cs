using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Build
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct BindingBuilderTo<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        #region Fields

        private readonly BindingBuilderFrom<TTarget, TSource> _fromBuilder;
        private ItemOrList<KeyValuePair<string?, object>, List<KeyValuePair<string?, object>>> _parameters;
        private readonly object _pathOrExpression;

        #endregion

        #region Constructors

        public BindingBuilderTo(BindingBuilderFrom<TTarget, TSource> from, object pathOrExpression, ItemOrList<KeyValuePair<string?, object>, List<KeyValuePair<string?, object>>> parameters)
        {
            Should.NotBeNull(pathOrExpression, nameof(pathOrExpression));
            _fromBuilder = from;
            _pathOrExpression = pathOrExpression;
            _parameters = parameters;
        }

        #endregion

        #region Methods

        public BindingBuilderTo<TTarget, TSource> BindingParameter(string? parameterName, object value)
        {
            Should.NotBeNull(value, nameof(value));
            var list = _parameters;
            list.Add(new KeyValuePair<string?, object>(parameterName, value), pair => pair.Value == null);
            _parameters = list;
            return this;
        }

        public static implicit operator ExpressionConverterRequest(BindingBuilderTo<TTarget, TSource> builder)
        {
            return new ExpressionConverterRequest(builder._fromBuilder.PathOrExpression, builder._pathOrExpression, builder._parameters.Cast<IReadOnlyList<KeyValuePair<string?, object>>>());
        }

        #endregion
    }
}