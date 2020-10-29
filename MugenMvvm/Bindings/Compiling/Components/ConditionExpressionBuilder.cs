using System.Linq.Expressions;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Compiling.Components
{
    public sealed class ConditionExpressionBuilder : IExpressionBuilderComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = CompilingComponentPriority.Condition;

        #endregion

        #region Implementation of interfaces

        public Expression? TryBuild(IExpressionBuilderContext context, IExpressionNode expression)
        {
            if (!(expression is IConditionExpressionNode condition))
                return null;

            var ifTrue = context.Build(condition.IfTrue);
            var ifFalse = context.Build(condition.IfFalse);
            MugenBindingExtensions.Convert(ref ifTrue, ref ifFalse, true);
            return Expression.Condition(context.Build(condition.Condition).ConvertIfNeed(typeof(bool), true), ifTrue, ifFalse);
        }

        #endregion
    }
}