using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class InlineBindingExpressionInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;

        #endregion

        #region Constructors

        public InlineBindingExpressionInitializer()
        {
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = BindingComponentPriority.BindingParameterInitializer;

        public bool UseOneTimeModeForStaticMembersImplicit { get; set; } = true;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.ContainsKey(BindingParameterNameConstant.EventHandler) || context.Components.ContainsKey(BindingParameterNameConstant.Mode) || !context.ParameterExpressions.IsEmpty)
                return;

            var collect = _memberExpressionCollectorVisitor.Collect(context.SourceExpression);
            var canInline = collect.Count == 0;
            if (!canInline && UseOneTimeModeForStaticMembersImplicit)
            {
                canInline = true;
                foreach (var parameter in collect)
                {
                    if (!parameter.MemberFlags.HasFlag(MemberFlags.Static))
                    {
                        canInline = false;
                        break;
                    }
                }
            }

            if (canInline)
                context.Components[BindingParameterNameConstant.Mode] = OneTimeBindingMode.Instance;
        }

        #endregion
    }
}