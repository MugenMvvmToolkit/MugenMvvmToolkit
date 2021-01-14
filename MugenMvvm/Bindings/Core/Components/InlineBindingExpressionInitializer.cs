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
        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;

        public InlineBindingExpressionInitializer()
        {
            _memberExpressionCollectorVisitor = new BindingMemberExpressionCollectorVisitor();
        }

        public bool UseOneTimeModeForStaticMembersImplicit { get; set; } = true;

        public int Priority { get; set; } = BindingComponentPriority.ParameterInitializer;

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            if (context.Components.ContainsKey(BindingParameterNameConstant.EventHandler) || context.Components.ContainsKey(BindingParameterNameConstant.Mode) ||
                !context.ParameterExpressions.IsEmpty)
                return;

            var expression = context.SourceExpression;
            var collect = _memberExpressionCollectorVisitor.Collect(ref expression, context.GetMetadataOrDefault());
            context.SourceExpression = expression;
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
    }
}