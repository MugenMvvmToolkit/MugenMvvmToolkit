using System;
using System.Linq.Expressions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;

namespace MugenMvvm.UnitTests.Bindings.Compiling.Internal
{
    public class TestExpressionBuilderComponent : IExpressionBuilderComponent
    {
        #region Properties

        public Func<IExpressionBuilderContext, IExpressionNode, Expression?>? TryBuild { get; set; }

        #endregion

        #region Implementation of interfaces

        Expression? IExpressionBuilderComponent.TryBuild(IExpressionBuilderContext context, IExpressionNode expression) => TryBuild?.Invoke(context, expression);

        #endregion
    }
}