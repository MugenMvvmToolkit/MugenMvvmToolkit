using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Compiling;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ExpressionCompilerCacheTest : UnitTestBase
    {
        public ExpressionCompilerCacheTest()
        {
            ExpressionCompiler.AddComponent(new ExpressionCompilerCache());
        }

        [Fact]
        public void ShouldCacheExpressionIgnoreBindingMemberExpression1()
        {
            var expression1 = new MemberExpressionNode(new BindingResourceMemberExpressionNode("R", "", 0, default, default), "T");
            var expression2 = new MemberExpressionNode(new BindingInstanceMemberExpressionNode(this, "", 0, default, default), "T");
            using var t = ExpressionCompiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (c, e, m) =>
                {
                    c.ShouldEqual(ExpressionCompiler);
                    e.ShouldEqual(expression1);
                    m.ShouldEqual(Metadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = ExpressionCompiler.Compile(expression1, Metadata);
            var memberPathObserver2 = ExpressionCompiler.Compile(expression2, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact]
        public void ShouldCacheExpressionIgnoreBindingMemberExpression2()
        {
            var expression1 = new BinaryExpressionNode(BinaryTokenType.Addition,
                new MemberExpressionNode(new BindingResourceMemberExpressionNode("R", "", 0, default, default), "T"),
                new BindingMemberExpressionNode("P1", 0, default, default));
            var expression2 = new BinaryExpressionNode(BinaryTokenType.Addition,
                new MemberExpressionNode(new BindingInstanceMemberExpressionNode(this, "", 0, default, default), "T"),
                new BindingMemberExpressionNode("P2", 0, default, default));
            using var t = ExpressionCompiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (c, e, m) =>
                {
                    c.ShouldEqual(ExpressionCompiler);
                    e.ShouldEqual(expression1);
                    m.ShouldEqual(Metadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = ExpressionCompiler.Compile(expression1, Metadata);
            var memberPathObserver2 = ExpressionCompiler.Compile(expression2, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact]
        public void ShouldCacheInvalidateExpression()
        {
            var expression = new MemberExpressionNode(ConstantExpressionNode.True, "T");
            using var t = ExpressionCompiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (c, e, m) =>
                {
                    c.ShouldEqual(ExpressionCompiler);
                    e.ShouldEqual(expression);
                    m.ShouldEqual(Metadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = ExpressionCompiler.Compile(expression, Metadata);
            var memberPathObserver2 = ExpressionCompiler.Compile(expression, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            ExpressionCompiler.TryInvalidateCache();
            memberPathObserver2 = ExpressionCompiler.Compile(expression, Metadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            memberPathObserver1 = ExpressionCompiler.Compile(expression, Metadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            ExpressionCompiler.TryInvalidateCache(expression);
            memberPathObserver2 = ExpressionCompiler.Compile(expression, Metadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact(Skip = ReleaseTest)]
        public void ShouldNotCacheGlobalRef()
        {
            var reference = ShouldNotCacheGlobalRefImpl();
            GcCollect();
            reference.IsAlive.ShouldBeFalse();
        }

        protected override IExpressionCompiler GetExpressionCompiler() => new ExpressionCompiler(ComponentCollectionManager);

        private WeakReference ShouldNotCacheGlobalRefImpl()
        {
            var obj = new object();
            var expression = new MemberExpressionNode(new BindingInstanceMemberExpressionNode(obj, "", 0, default, default), "T");
            using var t = ExpressionCompiler.AddComponent(new TestExpressionCompilerComponent
            {
                TryCompile = (_, _, _) => new TestCompiledExpression()
            });

            ExpressionCompiler.Compile(expression);
            return new WeakReference(obj, false);
        }
    }
}