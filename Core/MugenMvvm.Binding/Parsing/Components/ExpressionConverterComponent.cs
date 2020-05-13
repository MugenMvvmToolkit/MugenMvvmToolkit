using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Constants;
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

        private readonly ComponentTracker _componentTracker;
        private readonly ExpressionConverterContext<Expression> _context;

        #endregion

        #region Constructors

        public ExpressionConverterComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _context = new ExpressionConverterContext<Expression>(metadataContextProvider);
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IExpressionConverterComponent<Expression>, ExpressionConverterContext<Expression>>((components, state, _) => state.Converters = components, _context);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ParsingComponentPriority.Converter;

        #endregion

        #region Implementation of interfaces

        ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> IExpressionParserComponent.TryParse<TExpression>(in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            if (Default.IsValueType<TExpression>())
            {
                if (typeof(TExpression) == typeof(ExpressionConverterRequest))
                    return Parse(MugenExtensions.CastGeneric<TExpression, ExpressionConverterRequest>(expression), metadata);
            }
            else if (expression is IReadOnlyList<ExpressionConverterRequest> expressions)
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

            return default;
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetachedInternal(IExpressionParser owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }

        private ItemOrList<ExpressionParserResult, IReadOnlyList<ExpressionParserResult>> Parse(ExpressionConverterRequest expression, IReadOnlyMetadataContext? metadata)
        {
            _context.Initialize(metadata);
            var target = Convert(expression.Target, false);
            var source = Convert(expression.Source, false);

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

        private IExpressionNode Convert(object? expression, bool allowConstant, bool stringAsMember = true)
        {
            if (expression == null)
                return MemberExpressionNode.Empty;

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

            if (expression is IExpressionNode expressionNode)
                return expressionNode;
            if (allowConstant)
                return ConstantExpressionNode.Get(expression);
            if (stringAsMember && expression is string member)
                return MemberExpressionNode.Get(null, member);
            BindingExceptionManager.ThrowCannotParseExpression(expression);
            return null!;
        }

        private void AddParameter(KeyValuePair<string?, object> parameter, ref ItemOrList<IExpressionNode, List<IExpressionNode>> result)
        {
            if (parameter.Key == null)
            {
                if (parameter.Value != null)
                    result.Add(Convert(parameter.Value, false, false));
                return;
            }

            result.Add(new BinaryExpressionNode(BinaryTokenType.Assignment, MemberExpressionNode.Get(null, parameter.Key), Convert(parameter.Value, true)));
        }

        #endregion
    }
}