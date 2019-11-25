using System;
using System.Linq.Expressions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILambdaExpressionCompiler : IComponent<IMugenApplication>
    {
        Delegate Compile(LambdaExpression lambdaExpression);

        TDelegate Compile<TDelegate>(LambdaExpression lambdaExpression) where TDelegate : Delegate;
    }
}