﻿using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components
{
    public sealed class ExpressionParserConverter : AttachableComponentBase<IExpressionParser>, IExpressionParserComponent, IHasPriority
    {
        private readonly ComponentTracker _componentTracker;
        private readonly ExpressionConverterContext<Expression> _context;
        private readonly StringTokenParserContext _parserContext;

        [Preserve(Conditional = true)]
        public ExpressionParserConverter()
        {
            _context = new ExpressionConverterContext<Expression>();
            _parserContext = new StringTokenParserContext();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionConverterComponent<Expression>, ExpressionConverterContext<Expression>>(
                (components, state, _) => state.Converters = components, _context);
            _componentTracker.AddListener<ITokenParserComponent, StringTokenParserContext>((components, state, _) => state.Parsers = components, _parserContext);
        }

        public int Priority { get; init; } = ParsingComponentPriority.TokenParser;

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

        private ExpressionParserResult Parse(BindingExpressionRequest expression, IReadOnlyMetadataContext? metadata)
        {
            _context.Initialize(metadata);
            var target = Convert(expression.Target, metadata);
            var source = Convert(expression.Source, metadata);
            var parameters = new ItemOrListEditor<IExpressionNode>();
            foreach (var parameter in expression.Parameters)
                AddParameter(parameter, ref parameters, metadata);

            return new ExpressionParserResult(target, source, parameters.ToItemOrList());
        }

        private IExpressionNode Convert(object? expression, IReadOnlyMetadataContext? metadata = null)
        {
            if (expression == null)
                return MemberExpressionNode.Empty;

            if (expression is IExpressionNode expressionNode)
                return expressionNode;

            if (expression is Expression exp)
            {
                if (exp is not LambdaExpression lambdaExpression)
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

        private void AddParameter(KeyValuePair<string?, object> parameter, ref ItemOrListEditor<IExpressionNode> result, IReadOnlyMetadataContext? metadata)
        {
            if (parameter.Key != null)
                result.Add(new BinaryExpressionNode(BinaryTokenType.Assignment, MemberExpressionNode.Get(null, parameter.Key), Convert(parameter.Value, metadata)));
            else if (parameter.Value != null)
                result.Add(Convert(parameter.Value, metadata));
        }

        ItemOrIReadOnlyList<ExpressionParserResult> IExpressionParserComponent.TryParse(IExpressionParser parser, object expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is string stringExpression)
            {
                _parserContext.Initialize(stringExpression, metadata);
                return _parserContext.ParseExpression();
            }

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
                    result[i] = Parse(expressions[i], metadata);
                return result;
            }

            return default;
        }
    }
}