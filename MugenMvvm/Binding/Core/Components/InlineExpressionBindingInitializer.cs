using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Extensions.Internal;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class InlineExpressionBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Fields

        private readonly BindingMemberExpressionCollectorVisitor _memberExpressionCollectorVisitor;

        #endregion

        #region Constructors

        public InlineExpressionBindingInitializer()
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
            if (context.BindingComponents.ContainsKey(BindingParameterNameConstant.EventHandler)
                || context.BindingComponents.ContainsKey(BindingParameterNameConstant.Mode)
                || !MugenMvvm.Extensions.MugenExtensions.IsNullOrEmpty(context.Parameters))
                return;

            var collect = _memberExpressionCollectorVisitor.Collect(context.SourceExpression);
            var count = collect.Count();
            var canInline = count == 0;
            if (!canInline && UseOneTimeModeForStaticMembersImplicit)
            {
                canInline = true;
                for (var i = 0; i < count; i++)
                {
                    if (!collect.Get(i).MemberFlags.HasFlagEx(MemberFlags.Static))
                    {
                        canInline = false;
                        break;
                    }
                }
            }

            if (canInline)
                context.BindingComponents[BindingParameterNameConstant.Mode] = OneTimeBindingMode.Instance;
        }

        #endregion
    }
}