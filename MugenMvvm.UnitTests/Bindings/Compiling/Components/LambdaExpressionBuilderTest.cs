using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Components
{
    public class LambdaExpressionBuilderTest : ExpressionBuilderTestBase<LambdaExpressionBuilder>
    {
        [Fact]
        public void TryBuildShouldBuildLambdaExpression1()
        {
            var parameterInfo = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc1))!.GetParameters()[0]);
            var dictionary = new Dictionary<IExpressionNode, Expression>();

            Context.SetExpression = (node, ex) => dictionary[node] = ex;
            Context.ClearExpression = node => dictionary.Remove(node);
            Context.Build = node => dictionary[node];
            Context.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);

            var parameter1 = new ParameterExpressionNode("i");
            var lambdaExpressionNode = new LambdaExpressionNode(parameter1, new[] {parameter1});
            var expression = (Expression<Func<int, int>>) Builder.TryBuild(Context, lambdaExpressionNode)!;
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
            var dictionary = new Dictionary<IExpressionNode, Expression>();

            Context.SetExpression = (node, ex) => dictionary[node] = ex;
            Context.ClearExpression = node => dictionary.Remove(node);
            Context.Build = node => dictionary[node];
            Context.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);

            var parameter1 = new ParameterExpressionNode("i1");
            var parameter2 = new ParameterExpressionNode("i2");
            var lambdaExpressionNode = new LambdaExpressionNode(parameter2, new[] {parameter1, parameter2});
            var expression = (Expression<Func<int, int, int>>) Builder.TryBuild(Context, lambdaExpressionNode)!;
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
            Context.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfo);
            var lambdaExpressionNode = new LambdaExpressionNode(ConstantExpressionNode.False, Array.Empty<IParameterExpressionNode>());
            var expression = (Expression<Func<bool>>) Builder.TryBuild(Context, lambdaExpressionNode)!;
            expression.ShouldNotBeNull();
            expression.Compile().Invoke().ShouldEqual(false);
        }

        [Fact]
        public void TryBuildShouldIgnoreLambdaExpressionNoParameter() =>
            Builder.TryBuild(Context, new LambdaExpressionNode(ConstantExpressionNode.False, Array.Empty<IParameterExpressionNode>())).ShouldBeNull();

        [Fact]
        public void TryBuildShouldIgnoreLambdaExpressionWrongParameterCount()
        {
            var parameterInfoImpl = new ParameterInfoImpl(GetType().GetMethod(nameof(MethodWithFunc1))!.GetParameters()[0]);
            Context.Metadata.Set(CompilingMetadata.LambdaParameter, parameterInfoImpl);
            Builder.TryBuild(Context, new LambdaExpressionNode(ConstantExpressionNode.False, Array.Empty<IParameterExpressionNode>())).ShouldBeNull();
        }

        [Fact]
        public void TryBuildShouldIgnoreNotLambdaExpression() => Builder.TryBuild(Context, ConstantExpressionNode.False).ShouldBeNull();

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
    }
}