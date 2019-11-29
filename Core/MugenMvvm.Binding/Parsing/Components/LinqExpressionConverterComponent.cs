using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class LinqExpressionConverterComponent : ComponentTrackerBase<IExpressionParser, IExpressionConverterParserComponent<Expression>>, IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly ExpressionConverterContext<Expression> _context;
        private readonly FuncEx<ExpressionConverterRequest, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> _tryParseDelegate;

        #endregion

        #region Constructors

        public LinqExpressionConverterComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _context = new ExpressionConverterContext<Expression>(metadataContextProvider);
            _tryParseDelegate = Parse;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Converter;

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (_tryParseDelegate is FuncEx<TExpression, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> parser)
                return parser(expression, metadata);
            return default;
        }

        #endregion

        #region Methods

        protected override void OnComponentAdded(IComponentCollection<IComponent<IExpressionParser>> collection, IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            base.OnComponentAdded(collection, component, metadata);
            _context.Converters = Components;
        }

        protected override void OnComponentRemoved(IComponentCollection<IComponent<IExpressionParser>> collection, IComponent<IExpressionParser> component, IReadOnlyMetadataContext? metadata)
        {
            base.OnComponentRemoved(collection, component, metadata);
            _context.Converters = Components;
        }

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _context.Converters = Components;
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _context.Converters = Components;
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse(in ExpressionConverterRequest expression, IReadOnlyMetadataContext? metadata)
        {
            _context.Initialize(metadata);
            var target = _context.Convert(GetExpression(expression.Target));
            var source = expression.Source == null ? MemberExpressionNode.Empty : _context.Convert(GetExpression(expression.Source));

            var list = expression.Parameters.List;
            ItemOrList<IExpressionNode, List<IExpressionNode>> parameters = default;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    AddParameter(list[i], ref parameters);
            }
            else
                AddParameter(expression.Parameters.Item, ref parameters);

            return new ExpressionParserResult(target, source, parameters.Cast<IReadOnlyList<IExpressionNode>>(), metadata);
        }

        [return: NotNullIfNotNull("expression")]
        private static Expression? GetExpression(Expression? expression)
        {
            if (expression is LambdaExpression lambda)
                return lambda.Body;
            return expression;
        }

        private void AddParameter(KeyValuePair<string, object> parameter, ref ItemOrList<IExpressionNode, List<IExpressionNode>> result)
        {
            if (parameter.Key == null)
                return;
            IExpressionNode right;
            if (parameter.Value is Expression expression)
                right = _context.Convert(GetExpression(expression));
            else
                right = ConstantExpressionNode.Get(parameter.Value);
            result.Add(new BinaryExpressionNode(BinaryTokenType.Equality, new MemberExpressionNode(null, parameter.Key), right));
        }

        #endregion
    }
}