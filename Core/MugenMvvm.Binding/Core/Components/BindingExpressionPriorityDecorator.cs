using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionPriorityDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionBuilderComponent>, IBindingExpressionBuilderComponent, IComparer<IBindingExpression>, IHasPriority
    {
        #region Constructors

        public BindingExpressionPriorityDecorator()
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

        public int Priority { get; set; } = BindingComponentPriority.ExpressionPriorityDecorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>([DisallowNull]in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            var expressions = Components.TryBuildBindingExpression(expression, metadata);
            var list = expressions.List;
            if (expressions.Item != null || list == null)
                return expressions;

            if (list is IBindingExpression[] array)
                Array.Sort(array, this);
            else if (list is List<IBindingExpression> l)
                l.Sort(this);
            else
            {
                var result = list.ToArray();
                Array.Sort(result, this);
                return result;
            }

            return expressions;
        }

        int IComparer<IBindingExpression>.Compare(IBindingExpression x, IBindingExpression y)
        {
            return TryGetPriority(y).CompareTo(TryGetPriority(x));
        }

        #endregion

        #region Methods

        private int TryGetPriority(IBindingExpression expression)
        {
            if (expression is IHasPriority hasPriority)
                return hasPriority.Priority;
            if (expression is IHasTargetExpressionBindingExpression hasTargetExpression)
                return TryGetPriority(hasTargetExpression.TargetExpression);
            return 0;
        }

        private int TryGetPriority(IExpressionNode expression)
        {
            if (expression is IBindingMemberExpressionNode bindingMemberExpression && BindingMemberPriorities.TryGetValue(bindingMemberExpression.Path, out var priority))
                return priority;
            var node = expression.TryGetRootMemberExpression();
            if (node is IHasPriority hasPriority)
                return hasPriority.Priority;
            if (node is IMemberExpressionNode member && BindingMemberPriorities.TryGetValue(member.Member, out priority))
                return priority;
            return 0;
        }

        #endregion
    }
}