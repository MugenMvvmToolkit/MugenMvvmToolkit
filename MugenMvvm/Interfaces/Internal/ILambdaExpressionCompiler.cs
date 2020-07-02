using System;
using System.Linq.Expressions;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILambdaExpressionCompiler
    {
        Delegate Compile(LambdaExpression lambdaExpression);

        TDelegate Compile<TDelegate>(Expression<TDelegate> lambdaExpression) where TDelegate : Delegate;
    }
}