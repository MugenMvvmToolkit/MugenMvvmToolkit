using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Compiling.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Compiling.Components
{
    public class LambdaExpressionBuilderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryBuildShouldIgnoreNotLambdaExpression()
        {
            var component = new LambdaExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, ConstantExpressionNode.False).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreLambdaExpressionNoParameter()
        {
            var component = new LambdaExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            component.TryBuild(ctx, new LambdaExpressionNode(ConstantExpressionNode.False, Default.EmptyArray<IParameterExpressionNode>())).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreLambdaExpressionWrongParameterCount()
        {
            var parameterInfoImpl = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc1))!.GetParameters()[0]);
            var component = new LambdaExpressionBuilder();
            var ctx = new TestExpressionBuilderContext();
            ctx.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfoImpl);
            component.TryBuild(ctx, new LambdaExpressionNode(ConstantExpressionNode.False, Default.EmptyArray<IParameterExpressionNode>())).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldBuildLambdaExpression1()
        {
            var parameterInfo = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc1))!.GetParameters()[0]);
            var component = new LambdaExpressionBuilder();
            var dictionary = new Dictionary<IExpressionNode, Expression>();
            var ctx = new TestExpressionBuilderContext
            {
                SetExpression = (node, ex) => dictionary[node] = ex,
                ClearExpression = node => dictionary.Remove(node),
                Build = node => dictionary[node]
            };

            ctx.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);
            var parameter1 = new ParameterExpressionNode("i");
            var lambdaExpressionNode = new LambdaExpressionNode(parameter1, new[] { parameter1 });
            var expression = (Expression<Func<int, int>>)component.TryBuild(ctx, lambdaExpressionNode)!;
            dictionary.Count.ShouldEqual(0);
            expression.ShouldNotBeNull();

            var compile = expression.Compile();
            compile.Invoke(1).ShouldEqual(1);
            compile.Invoke(5).ShouldEqual(5);
        }

        [Fact]
        public void TryBuildShouldBuildLambdaExpression2()
        {
            var parameterInfo = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc2))!.GetParameters()[0]);
            var component = new LambdaExpressionBuilder();
            var dictionary = new Dictionary<IExpressionNode, Expression>();
            var ctx = new TestExpressionBuilderContext
            {
                SetExpression = (node, ex) => dictionary[node] = ex,
                ClearExpression = node => dictionary.Remove(node),
                Build = node => dictionary[node]
            };

            ctx.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);
            var parameter1 = new ParameterExpressionNode("i1");
            var parameter2 = new ParameterExpressionNode("i2");
            var lambdaExpressionNode = new LambdaExpressionNode(parameter2, new[] { parameter1, parameter2 });
            var expression = (Expression<Func<int, int, int>>)component.TryBuild(ctx, lambdaExpressionNode)!;
            dictionary.Count.ShouldEqual(0);
            expression.ShouldNotBeNull();

            var compile = expression.Compile();
            compile.Invoke(0, 1).ShouldEqual(1);
            compile.Invoke(0, 5).ShouldEqual(5);
        }

        [Fact]
        public void TryBuildShouldBuildLambdaExpression3()
        {
            var parameterInfo = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc3))!.GetParameters()[0]);
            var component = new LambdaExpressionBuilder();
            var dictionary = new Dictionary<IExpressionNode, Expression>();
            var ctx = new TestExpressionBuilderContext();

            ctx.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);
            var lambdaExpressionNode = new LambdaExpressionNode(ConstantExpressionNode.False, Default.EmptyArray<IParameterExpressionNode>());
            var expression = (Expression<Func<bool>>)component.TryBuild(ctx, lambdaExpressionNode)!;
            dictionary.Count.ShouldEqual(0);
            expression.ShouldNotBeNull();
            expression.Compile().Invoke().ShouldEqual(false);
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public static void MethodWithFunc1(Func<int, int> func)
        {
        }

        public static void MethodWithFunc2(Func<int, int, int> func)
        {
        }

        public static void MethodWithFunc3(Func<bool> func)
        {
        }
#pragma warning restore xUnit1013 // Public method should be marked as test

        #endregion
    }
}