using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing
{
    public class TokenParserContextTest : MetadataOwnerTestBase
    {
        #region Fields

        private const string SourceString = "Test2423";

        #endregion

        #region Methods

        [Fact]
        public void InitializeShouldClearPrevValues()
        {
            var context = new TokenParserContext();
            context.Metadata.Set(BindingMetadata.EventArgs, "");
            context.Initialize(SourceString, DefaultMetadata);
            context.Position = 1;

            context.Initialize(SourceString, DefaultMetadata);
            context.Position.ShouldEqual(0);
            context.Metadata.Get(BindingMetadata.EventArgs).ShouldBeNull();
        }

        [Fact]
        public void LengthShouldUseLimit()
        {
            var context = new TokenParserContext();
            context.Initialize(SourceString, DefaultMetadata);
            context.Length.ShouldEqual(SourceString.Length);
            context.Limit = 1;
            context.Length.ShouldEqual(1);
            context.Limit = null;
            context.Length.ShouldEqual(SourceString.Length);
        }

        [Fact]
        public void LimitShouldThrowWrongValue()
        {
            var context = new TokenParserContext();
            context.Initialize(SourceString, DefaultMetadata);
            ShouldThrow<ArgumentException>(() => context.Limit = int.MaxValue);
            ShouldThrow<ArgumentException>(() => context.Limit = -1);
        }

        [Fact]
        public void PositionShouldThrowWrongValue()
        {
            var context = new TokenParserContext();
            context.Initialize(SourceString, DefaultMetadata);
            ShouldThrow<ArgumentException>(() => context.Limit = int.MaxValue);
            ShouldThrow<ArgumentException>(() => context.Limit = -1);
        }

        [Fact]
        public void TokenAtShouldUsePosition()
        {
            var context = new TokenParserContext();
            context.Initialize(SourceString, DefaultMetadata);
            context.TokenAt(0).ShouldEqual(SourceString[0]);
            context.TokenAt(4).ShouldEqual(SourceString[4]);
        }

        [Fact]
        public void GetValueShouldSubstring()
        {
            var context = new TokenParserContext();
            context.Initialize(SourceString, DefaultMetadata);
            context.GetValue(1, 3).ShouldEqual("es");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryParseShouldBeHandledByParsers(int componentCount)
        {
            var invokeCount = 0;
            var context = new TokenParserContext();
            var list = new List<TestTokenParserComponent>();
            var constantExpression = ConstantExpressionNode.False;
            var result = ConstantExpressionNode.Null;
            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i == componentCount - 1;
                var component = new TestTokenParserComponent
                {
                    Priority = -i,
                    TryParse = (ctx, ex) =>
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

            context.Parsers = list.ToArray();

            context.TryParse(constantExpression).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryParseShouldBeHandledByParsersWithCondition(int componentCount)
        {
            var invokeCount = 0;
            var context = new TokenParserContext();
            var list = new List<TestTokenParserComponent>();
            var constantExpression = ConstantExpressionNode.False;
            var result = ConstantExpressionNode.Null;
            for (var i = 0; i < componentCount; i++)
            {
                var canReturn = i == 0;
                var component = new TestTokenParserComponent
                {
                    Priority = -i,
                    TryParse = (ctx, ex) =>
                    {
                        ++invokeCount;
                        ctx.ShouldEqual(context);
                        ex.ShouldEqual(constantExpression);
                        if (canReturn)
                            return result;
                        return null;
                    }
                };
                list.Add(component);
            }

            context.Parsers = list.ToArray();

            context.TryParse(constantExpression, (parserContext, component) =>
            {
                context.ShouldEqual(context);
                return ((IHasPriority) component).Priority == 0;
            }).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            var ctx = new TokenParserContext();
            ctx.Initialize("Test", metadata);
            return ctx;
        }

        #endregion
    }
}