using System.Collections.Generic;
using System.Runtime.InteropServices;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Build
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct BindingBuilderTo<TTarget, TSource>
        where TTarget : class
        where TSource : class
    {
        #region Fields

        private readonly BindingBuilderFrom<TTarget, TSource> _fromBuilder;
        private ItemOrListEditor<KeyValuePair<string?, object>, List<KeyValuePair<string?, object>>> _parameters;
        private readonly object _pathOrExpression;

        #endregion

        #region Constructors

        public BindingBuilderTo(BindingBuilderFrom<TTarget, TSource> from, object pathOrExpression, ItemOrList<KeyValuePair<string?, object>, List<KeyValuePair<string?, object>>> parameters)
        {
            Should.NotBeNull(pathOrExpression, nameof(pathOrExpression));
            _fromBuilder = from;
            _pathOrExpression = pathOrExpression;
            _parameters = parameters.Editor(pair => pair.Value == null);
        }

        #endregion

        #region Methods

        public BindingBuilderTo<TTarget, TSource> BindingParameter(string? parameterName, object value)
        {
            Should.NotBeNull(value, nameof(value));
            _parameters.Add(new KeyValuePair<string?, object>(parameterName, value));
            return this;
        }

        public static implicit operator BindingExpressionRequest(BindingBuilderTo<TTarget, TSource> builder) => new BindingExpressionRequest(builder._fromBuilder.PathOrExpression, builder._pathOrExpression,
            builder._parameters.ToItemOrList<IReadOnlyList<KeyValuePair<string?, object>>>());

        #endregion
    }
}