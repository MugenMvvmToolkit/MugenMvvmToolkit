using System;
using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;

namespace MugenMvvm.UnitTest.Binding.Compiling.Internal
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