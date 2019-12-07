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
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Parsing.Components
{
    public sealed class ExpressionConverterComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly ExpressionConverterContext<Expression> _context;
        private readonly FuncEx<ExpressionConverterRequest, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> _tryParseDelegate;
        private readonly FuncEx<IReadOnlyList<ExpressionConverterRequest>, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> _tryParseListDelegate;

        #endregion

        #region Constructors

        public ExpressionConverterComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _context = new ExpressionConverterContext<Expression>(metadataContextProvider);
            _tryParseDelegate = Parse;
            _tryParseListDelegate = Parse;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Converter;

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (_tryParseDelegate is FuncEx<TExpression, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> parser1)
                return parser1(expression, metadata);
            if (_tryParseListDelegate is FuncEx<TExpression, IReadOnlyMetadataContext?, ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>>> parser2)
                return parser2(expression, metadata);
            return default;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _context.Owner = owner;
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _context.Owner = null;
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse(in IReadOnlyList<ExpressionConverterRequest> expressions, IReadOnlyMetadataContext? metadata)
        {
            if (expressions.Count == 0)
                return default;
            if (expressions.Count == 1)
                return Parse(expressions[0], metadata);

            var result = new ExpressionParserResult[expressions.Count];
            for (var i = 0; i < result.Length; i++)
                result[i] = Parse(expressions[i], metadata).Item;
            return result;
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

            return new ExpressionParserResult(target, source, parameters.Cast<IReadOnlyList<IExpressionNode>>());
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