using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Parsing.Components
{
    public sealed class ExpressionConverterComponent : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private readonly ExpressionConverterContext<Expression> _context;
        private readonly TokenParserContext _parserContext;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ExpressionConverterComponent()
        {
            _context = new ExpressionConverterContext<Expression>();
            _parserContext = new TokenParserContext();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionConverterComponent<Expression>, ExpressionConverterContext<Expression>>((components, state, _) => state.Converters = components, _context);
            _componentTracker.AddListener<ITokenParserComponent, TokenParserContext>((components, state, _) => state.Parsers = components, _parserContext);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Converter;

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is BindingExpressionRequest request)
                return Parse(request, metadata);

            if (expression is IReadOnlyList<BindingExpressionRequest> expressions)
            {
                if (expressions.Count == 0)
                    return default;
                if (expressions.Count == 1)
                    return Parse(expressions[0], metadata);

                var result = new ExpressionParserResult[expressions.Count];
                for (var i = 0; i < result.Length; i++)
                    result[i] = Parse(expressions[i], metadata).Item;
                return ItemOrList.FromListToReadOnly(result);
            }

            return default;
        }

        #endregion

        #region Methods

        protected override void OnAttached(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetached(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse(BindingExpressionRequest expression, IReadOnlyMetadataContext? metadata)
        {
            _context.Initialize(metadata);
            var target = Convert(expression.Target, metadata);
            var source = Convert(expression.Source, metadata);

            var list = expression.Parameters.List;
            var parameters = ItemOrListEditor.Get<IExpressionNode>();
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    AddParameter(list[i], ref parameters, metadata);
            }
            else
                AddParameter(expression.Parameters.Item, ref parameters, metadata);

            return new ExpressionParserResult(target, source, parameters.ToItemOrList<IReadOnlyList<IExpressionNode>>());
        }

        private IExpressionNode Convert(object? expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return MemberExpressionNode.Empty;

            if (expression is IExpressionNode expressionNode)
                return expressionNode;

            if (expression is Expression exp)
            {
                if (!(exp is LambdaExpression lambdaExpression))
                    return _context.Convert(exp);

                var parameters = lambdaExpression.Parameters;
                try
                {
                    for (var i = 0; i < parameters.Count; i++)
                        _context.SetExpression(parameters[i], ConstantExpressionNode.Null);
                    return _context.Convert(lambdaExpression.Body);
                }
                finally
                {
                    for (var i = 0; i < parameters.Count; i++)
                        _context.ClearExpression(parameters[i]);
                }
            }

            if (expression is string stExp)
            {
                if (string.IsNullOrEmpty(stExp))
                    return MemberExpressionNode.Empty;
                _parserContext.Initialize(stExp, metadata);
                return _parserContext.ParseWhileAnyOf(null);
            }

            ExceptionManager.ThrowCannotParseExpression(expression);
            return null!;
        }

        private void AddParameter(KeyValuePair<string?, object> parameter, ref ItemOrListEditor<IExpressionNode, List<IExpressionNode>> result, IReadOnlyMetadataContext? metadata)
        {
            if (parameter.Key != null)
                result.Add(new BinaryExpressionNode(BinaryTokenType.Assignment, MemberExpressionNode.Get(null, parameter.Key), Convert(parameter.Value, metadata), null));
            else if (parameter.Value != null)
                result.Add(Convert(parameter.Value, metadata));
        }

        #endregion
    }
}