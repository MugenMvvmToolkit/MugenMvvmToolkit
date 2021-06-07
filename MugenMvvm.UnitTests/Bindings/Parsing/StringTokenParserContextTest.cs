using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing
{
    public class StringTokenParserContextTest : MetadataOwnerTestBase
    {
        private const string SourceString = "Test2423";

        private readonly StringTokenParserContext _context;

        public StringTokenParserContextTest()
        {
            _context = new StringTokenParserContext();
            _context.Initialize(SourceString, DefaultMetadata);
        }

        [Fact]
        public void GetValueShouldSubstring()
        {
            _context.GetValue(1, 3).ShouldEqual("es");
        }

        [Fact]
        public void InitializeShouldClearPrevValues()
        {
            _context.Metadata.Set(BindingMetadata.EventArgs, "");
            _context.Position = 1;

            _context.Initialize(SourceString, DefaultMetadata);
            _context.Position.ShouldEqual(0);
            _context.Metadata.Get(BindingMetadata.EventArgs).ShouldBeNull();
        }

        [Fact]
        public void LengthShouldUseLimit()
        {
            _context.Length.ShouldEqual(SourceString.Length);
            _context.Limit = 1;
            _context.Length.ShouldEqual(1);
            _context.Limit = null;
            _context.Length.ShouldEqual(SourceString.Length);
        }

        [Fact]
        public void LimitShouldThrowWrongValue()
        {
            ShouldThrow<ArgumentException>(() => _context.Limit = int.MaxValue);
            ShouldThrow<ArgumentException>(() => _context.Limit = -1);
        }

        [Fact]
        public void PositionShouldThrowWrongValue()
        {
            ShouldThrow<ArgumentException>(() => _context.Limit = int.MaxValue);
            ShouldThrow<ArgumentException>(() => _context.Limit = -1);
        }

        [Fact]
        public void TokenAtShouldUsePosition()
        {
            _context.TokenAt(0).ShouldEqual(SourceString[0]);
            _context.TokenAt(4).ShouldEqual(SourceString[4]);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryParseShouldBeHandledByParsers(int componentCount)
        {
            var invokeCount = 0;
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
                        ctx.ShouldEqual(_context);
                        ex.ShouldEqual(constantExpression);
                        if (isLast)
                            return result;
                        return null;
                    }
                };
                list.Add(component);
            }

            _context.Parsers = list.ToArray();
            _context.TryParse(constantExpression).ShouldEqual(result);
            invokeCount.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(100)]
        public void TryParseShouldBeHandledByParsersWithCondition(int componentCount)
        {
            var invokeCount = 0;
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
                        ctx.ShouldEqual(_context);
                        ex.ShouldEqual(constantExpression);
                        if (canReturn)
                            return result;
                        return null;
                    }
                };
                list.Add(component);
            }

            _context.Parsers = list.ToArray();
            _context.TryParse(constantExpression, (parserContext, component) =>
            {
                parserContext.ShouldEqual(_context);
                return ((IHasPriority) component).Priority == 0;
            }).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            var ctx = new StringTokenParserContext();
            ctx.Initialize("Test", metadata);
            return ctx;
        }
    }
}