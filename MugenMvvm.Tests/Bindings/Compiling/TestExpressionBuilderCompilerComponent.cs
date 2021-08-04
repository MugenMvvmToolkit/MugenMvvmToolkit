using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.Tests.Bindings.Compiling
{
    public class TestExpressionBuilderComponent : IExpressionBuilderComponent
    {
        public Func<IExpressionBuilderContext, IExpressionNode, Expression?>? TryBuild { get; set; }

        Expression? IExpressionBuilderComponent.TryBuild(IExpressionBuilderContext context, IExpressionNode expression) => TryBuild?.Invoke(context, expression);
    }
}