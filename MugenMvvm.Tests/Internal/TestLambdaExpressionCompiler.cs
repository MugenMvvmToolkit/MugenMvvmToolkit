using System;
using System.Linq.Expressions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestLambdaExpressionCompiler : ILambdaExpressionCompiler
    {
        public Func<LambdaExpression, Delegate?>? Compile { get; set; }

        public Func<LambdaExpression, Type, Delegate?>? CompileGeneric { get; set; }

        Delegate ILambdaExpressionCompiler.Compile(LambdaExpression lambdaExpression) => Compile?.Invoke(lambdaExpression) ?? lambdaExpression.Compile();

        TDelegate ILambdaExpressionCompiler.Compile<TDelegate>(Expression<TDelegate> lambdaExpression) =>
            (TDelegate)CompileGeneric?.Invoke(lambdaExpression, typeof(TDelegate))! ?? lambdaExpression.Compile();
    }
}