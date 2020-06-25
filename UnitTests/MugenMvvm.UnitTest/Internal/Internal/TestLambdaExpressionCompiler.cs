using System;
using System.Linq.Expressions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestLambdaExpressionCompiler : ILambdaExpressionCompiler
    {
        #region Properties

        public Func<LambdaExpression, Delegate?>? Compile { get; set; }

        public Func<LambdaExpression, Type, Delegate?>? CompileGeneric { get; set; }

        #endregion

        #region Implementation of interfaces

        Delegate ILambdaExpressionCompiler.Compile(LambdaExpression lambdaExpression)
        {
            return Compile?.Invoke(lambdaExpression) ?? lambdaExpression.Compile();
        }

        TDelegate ILambdaExpressionCompiler.Compile<TDelegate>(Expression<TDelegate> lambdaExpression)
        {
            return (TDelegate) CompileGeneric?.Invoke(lambdaExpression, typeof(TDelegate))! ?? lambdaExpression.Compile();
        }

        #endregion
    }
}