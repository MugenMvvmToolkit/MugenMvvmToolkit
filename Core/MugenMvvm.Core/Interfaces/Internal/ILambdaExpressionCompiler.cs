using System;
using System.Linq.Expressions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface ILambdaExpressionCompiler : IComponent<IMugenApplication>
    {
        Delegate Compile(LambdaExpression lambdaExpression, IReadOnlyMetadataContext? metadata);

        TDelegate Compile<TDelegate>(LambdaExpression lambdaExpression, IReadOnlyMetadataContext? metadata) where TDelegate : Delegate;
    }
}