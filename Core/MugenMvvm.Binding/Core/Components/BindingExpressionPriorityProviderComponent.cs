using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionPriorityProviderComponent : IBindingExpressionPriorityProviderComponent, IHasPriority
    {
        #region Constructors

        public BindingExpressionPriorityProviderComponent()
        {
            BindingMemberPriorities = new Dictionary<string, int>
            {
                {BindableMembers.Object.DataContext, BindableMemberPriority.DataContext},
                {"BindingContext", BindableMemberPriority.DataContext},
                {"ItemTemplate", BindableMemberPriority.Template},
                {"ItemTemplateSelector", BindableMemberPriority.Template},
                {"ContentTemplate", BindableMemberPriority.Template},
                {"ContentTemplateSelector", BindableMemberPriority.Template}
            };
        }

        #endregion

        #region Properties

        public Dictionary<string, int> BindingMemberPriorities { get; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool TryGetPriority(IExpressionNode expression, out int priority)
        {
            var node = expression.TryGetRootMemberExpression();
            if (node is IHasPriority hasPriority)
            {
                priority = hasPriority.Priority;
                return true;
            }

            if (node is IMemberExpressionNode member)
                return BindingMemberPriorities.TryGetValue(member.MemberName, out priority);

            priority = 0;
            return false;
        }

        #endregion
    }
}