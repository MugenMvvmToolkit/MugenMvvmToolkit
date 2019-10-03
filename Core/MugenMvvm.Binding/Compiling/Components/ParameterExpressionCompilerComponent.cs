﻿using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ParameterExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryCompile(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (expression is IParameterExpression parameter)
                return context.GetParameterExpression(parameter);
            return null;
        }

        #endregion
    }
}