﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Parsing.Components.Converters
{
    public sealed class BinaryExpressionConverter : IExpressionConverterComponent<Expression>, IHasPriority
    {
        public BinaryExpressionConverter()
        {
            Mapping = new Dictionary<ExpressionType, Func<BinaryExpression, BinaryTokenType>>(23)
            {
                {ExpressionType.Multiply, _ => BinaryTokenType.Multiplication},
                {ExpressionType.Divide, _ => BinaryTokenType.Division},
                {ExpressionType.Modulo, _ => BinaryTokenType.Remainder},
                {ExpressionType.Add, _ => BinaryTokenType.Addition},
                {ExpressionType.Subtract, _ => BinaryTokenType.Subtraction},
                {ExpressionType.LeftShift, _ => BinaryTokenType.LeftShift},
                {ExpressionType.RightShift, _ => BinaryTokenType.RightShift},
                {ExpressionType.LessThan, _ => BinaryTokenType.LessThan},
                {ExpressionType.GreaterThan, _ => BinaryTokenType.GreaterThan},
                {ExpressionType.LessThanOrEqual, _ => BinaryTokenType.LessThanOrEqual},
                {ExpressionType.GreaterThanOrEqual, _ => BinaryTokenType.GreaterThanOrEqual},
                {ExpressionType.Equal, _ => BinaryTokenType.Equality},
                {ExpressionType.NotEqual, _ => BinaryTokenType.NotEqual},
                {ExpressionType.And, _ => BinaryTokenType.LogicalAnd},
                {ExpressionType.ExclusiveOr, _ => BinaryTokenType.LogicalXor},
                {ExpressionType.Or, _ => BinaryTokenType.LogicalOr},
                {ExpressionType.AndAlso, _ => BinaryTokenType.ConditionalAnd},
                {ExpressionType.OrElse, _ => BinaryTokenType.ConditionalOr},
                {ExpressionType.Coalesce, _ => BinaryTokenType.NullCoalescing}
            };
        }

        public Dictionary<ExpressionType, Func<BinaryExpression, BinaryTokenType>> Mapping { get; }

        public int Priority { get; init; } = ParsingComponentPriority.Binary;

        public IExpressionNode? TryConvert(IExpressionConverterContext<Expression> context, Expression expression)
        {
            if (expression is BinaryExpression binary && Mapping.TryGetValue(binary.NodeType, out var func))
                return new BinaryExpressionNode(func(binary), context.Convert(binary.Left), context.Convert(binary.Right));

            return null;
        }
    }
}