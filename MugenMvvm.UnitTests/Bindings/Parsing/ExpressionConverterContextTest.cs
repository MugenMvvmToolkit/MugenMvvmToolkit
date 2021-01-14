﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Parsing.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing
{
    public class ExpressionConverterContextTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConvertShouldReturnExpression()
        {
            var constantExpression = Expression.Constant("");
            var result = ConstantExpressionNode.Null;
            var context = new ExpressionConverterContext<Expression>();
            context.SetExpression(constantExpression, result);
            context.Convert(constantExpression).ShouldEqual(result);
        }

        [Fact]
        public void ConvertShouldThrowEmpty()
        {
            var context = new ExpressionConverterContext<Expression>();
            ShouldThrow<InvalidOperationException>(() => context.Convert(Expression.Constant("")));
        }

        [Fact]
        public void InitializeShouldClearPrevValues()
        {
            var constantExpression = Expression.Constant("");
            var result = ConstantExpressionNode.Null;
            var context = new ExpressionConverterContext<Expression>();
            context.SetExpression(constantExpression, result);
            context.Metadata.Set(BindingMetadata.EventArgs, "");

            context.Initialize(DefaultMetadata);
            context.TryGetExpression(constantExpression).ShouldBeNull();
            context.Metadata.Get(BindingMetadata.EventArgs).ShouldBeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryGetSetClearExpressionShouldUpdateExpressions(int count)
        {
            var context = new ExpressionConverterContext<Expression>();
            context.TryGetExpression(Expression.Constant(0)).ShouldBeNull();

            var valueTuples = new List<(Expression, IExpressionNode)>();
            for (var i = 0; i < count; i++)
                valueTuples.Add((Expression.Constant(i), ConstantExpressionNode.Get(i)));

            for (var i = 0; i < count; i++)
                context.SetExpression(valueTuples[i].Item1, valueTuples[i].Item2);

            for (var i = 0; i < count; i++)
                context.TryGetExpression(valueTuples[i].Item1).ShouldEqual(valueTuples[i].Item2);

            for (var i = 0; i < count; i++)
            {
                context.TryGetExpression(valueTuples[i].Item1).ShouldEqual(valueTuples[i].Item2);
                context.ClearExpression(valueTuples[i].Item1);
                context.TryGetExpression(valueTuples[i].Item1).ShouldBeNull();
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void ConvertShouldBeHandledByConverters(int componentCount)
        {
            var invokeCount = 0;
            var context = new ExpressionConverterContext<Expression>();
            var list = new List<IExpressionConverterComponent<Expression>>();
            var constantExpression = Expression.Constant("");
            var result = ConstantExpressionNode.Null;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestExpressionConverterComponent<Expression>
                {
                    Priority = -i,
                    TryConvert = (ctx, ex) =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(context);
                        ex.ShouldEqual(constantExpression);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                list.Add(component);
            }

            context.Converters = list.ToArray();

            context.Convert(constantExpression).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            var ctx = new ExpressionConverterContext<Expression>();
            ctx.Initialize(metadata);
            return ctx;
        }
    }
}