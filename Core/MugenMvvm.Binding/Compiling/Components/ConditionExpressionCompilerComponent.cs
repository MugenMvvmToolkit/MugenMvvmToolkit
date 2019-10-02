using System.Linq.Expressions;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class ConditionExpressionCompilerComponent : ExpressionCompilerComponent.ICompiler, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public Expression? TryCompile(ExpressionCompilerComponent.IContext context, IExpressionNode expression)
        {
            if (!(expression is IConditionExpressionNode condition))
                return null;

            var ifTrue = context.Compile(condition.IfTrue);
            var ifFalse = context.Compile(condition.IfFalse);
            BindingMugenExtensions.Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(context.Compile(condition.Condition).ConvertIfNeed(typeof(bool), true), ifTrue, ifFalse);
        }

        #endregion
    }
}