using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Parsing
{
    public sealed class ExpressionConverterContext<TExpression> : MetadataOwnerBase, IExpressionConverterContext<TExpression> where TExpression : class
    {
        #region Fields

        private readonly Dictionary<TExpression, IExpressionNode?> _expressions;
        private IExpressionConverterComponent<TExpression>[] _converters;

        #endregion

        #region Constructors

        public ExpressionConverterContext() : base(null)
        {
            _expressions = new Dictionary<TExpression, IExpressionNode?>();
            _converters = Default.Array<IExpressionConverterComponent<TExpression>>();
        }

        #endregion

        #region Properties

        public IExpressionConverterComponent<TExpression>[] Converters
        {
            get => _converters;
            set
            {
                Should.NotBeNull(value, nameof(value));
                _converters = value;
            }
        }

        #endregion

        #region Implementation of interfaces

        public IExpressionNode? TryGetExpression(TExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressions.TryGetValue(expression, out var value);
            return value;
        }

        public void SetExpression(TExpression expression, IExpressionNode value)
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(value, nameof(value));
            _expressions[expression] = value;
        }

        public void ClearExpression(TExpression expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressions.Remove(expression);
        }

        public IExpressionNode? TryConvert(TExpression expression) => _converters.TryConvert(this, expression) ?? TryGetExpression(expression);

        #endregion

        #region Methods

        public void Initialize(IReadOnlyMetadataContext? metadata)
        {
            _expressions.Clear();
            MetadataRaw?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
        }

        #endregion
    }
}