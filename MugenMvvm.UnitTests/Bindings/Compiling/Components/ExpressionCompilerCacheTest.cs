using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions.Binding;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Compiling.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class ExpressionCompilerCacheTest : UnitTestBase
    {
        #region Fields

        private readonly ExpressionCompiler _compiler;

        #endregion

        #region Constructors

        public ExpressionCompilerCacheTest()
        {
            _compiler = new ExpressionCompiler();
            _compiler.AddComponent(new ExpressionCompilerCache());
        }

        #endregion

        #region Methods

        [Fact]
        public void ShouldCacheInvalidateExpression()
        {
            var expression = new MemberExpressionNode(ConstantExpressionNode.True, "T");
            using var t = _compiler.AddComponent(new TestExpressionCompilerComponent(_compiler)
            {
                TryCompile = (e, m) =>
                {
                    e.ShouldEqual(expression);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = _compiler.Compile(expression, DefaultMetadata);
            var memberPathObserver2 = _compiler.Compile(expression, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            _compiler.TryInvalidateCache();
            memberPathObserver2 = _compiler.Compile(expression, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            memberPathObserver1 = _compiler.Compile(expression, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);

            _compiler.TryInvalidateCache(expression);
            memberPathObserver2 = _compiler.Compile(expression, DefaultMetadata);
            memberPathObserver1.ShouldNotEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact]
        public void ShouldCacheExpressionIgnoreBindingMemberExpression1()
        {
            var expression1 = new MemberExpressionNode(new BindingResourceMemberExpressionNode("R", "", 0, default, default), "T");
            var expression2 = new MemberExpressionNode(new BindingInstanceMemberExpressionNode(this, "", 0, default, default), "T");
            using var t = _compiler.AddComponent(new TestExpressionCompilerComponent(_compiler)
            {
                TryCompile = (e, m) =>
                {
                    e.ShouldEqual(expression1);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = _compiler.Compile(expression1, DefaultMetadata);
            var memberPathObserver2 = _compiler.Compile(expression2, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact]
        public void ShouldCacheExpressionIgnoreBindingMemberExpression2()
        {
            var expression1 = new BinaryExpressionNode(BinaryTokenType.Addition, new MemberExpressionNode(new BindingResourceMemberExpressionNode("R", "", 0, default, default), "T"),
                new BindingMemberExpressionNode("P1", 0, default, default));
            var expression2 = new BinaryExpressionNode(BinaryTokenType.Addition, new MemberExpressionNode(new BindingInstanceMemberExpressionNode(this, "", 0, default, default), "T"),
                new BindingMemberExpressionNode("P2", 0, default, default));
            using var t = _compiler.AddComponent(new TestExpressionCompilerComponent(_compiler)
            {
                TryCompile = (e, m) =>
                {
                    e.ShouldEqual(expression1);
                    m.ShouldEqual(DefaultMetadata);
                    return new TestCompiledExpression();
                }
            });

            var memberPathObserver1 = _compiler.Compile(expression1, DefaultMetadata);
            var memberPathObserver2 = _compiler.Compile(expression2, DefaultMetadata);
            memberPathObserver1.ShouldEqual(memberPathObserver2, ReferenceEqualityComparer.Instance);
        }

        [Fact(Skip = ReleaseTest)]
        public void ShouldNotCacheGlobalRef()
        {
            var reference = ShouldNotCacheGlobalRefImpl();
            GcCollect();
            reference.IsAlive.ShouldBeFalse();
        }

        private WeakReference ShouldNotCacheGlobalRefImpl()
        {
            var obj = new object();
            var expression = new MemberExpressionNode(new BindingInstanceMemberExpressionNode(obj, "", 0, default, default), "T");
            using var t = _compiler.AddComponent(new TestExpressionCompilerComponent(_compiler)
            {
                TryCompile = (_, _) => new TestCompiledExpression()
            });

            _compiler.Compile(expression);
            return new WeakReference(obj, false);
        }

        #endregion
    }
}