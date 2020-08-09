using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingExpressionPriorityDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent, IComparer<IBindingBuilder>, IHasPriority
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public BindingExpressionPriorityDecorator()
        {
            BindingMemberPriorities = new Dictionary<string, int>
            {
                {BindableMembers.For<object>().DataContext(), BindableMemberPriority.DataContext},
                {BindingInternalConstant.BindingContext, BindableMemberPriority.DataContext},
                {BindingInternalConstant.ItemTemplate, BindableMemberPriority.Template},
                {BindingInternalConstant.ItemTemplateSelector, BindableMemberPriority.Template},
                {BindingInternalConstant.ContentTemplate, BindableMemberPriority.Template},
                {BindingInternalConstant.ContentTemplateSelector, BindableMemberPriority.Template},
                {BindingInternalConstant.StableIdProvider, BindableMemberPriority.Template},
                {BindingInternalConstant.ItemsSource, BindableMemberPriority.ItemsSource}
            };
        }

        #endregion

        #region Properties

        public Dictionary<string, int> BindingMemberPriorities { get; }

        public int FakeMemberPriority { get; set; } = BindableMemberPriority.Fake;

        public int Priority { get; set; } = BindingComponentPriority.ExpressionPriorityDecorator;

        #endregion

        #region Implementation of interfaces

        public ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            var expressions = Components.TryParseBindingExpression(bindingManager, expression, metadata);
            var list = expressions.List;
            if (expressions.Item != null || list == null)
                return expressions;

            if (list is IBindingBuilder[] array)
                Array.Sort(array, this);
            else if (list is List<IBindingBuilder> l)
                l.Sort(this);
            else
            {
                var result = list.ToArray();
                Array.Sort(result, this);
                return ItemOrList.FromListToReadOnly(result);
            }

            return expressions;
        }

        int IComparer<IBindingBuilder>.Compare([AllowNull] IBindingBuilder x, [AllowNull] IBindingBuilder y) => TryGetPriority(y!).CompareTo(TryGetPriority(x!));

        #endregion

        #region Methods

        private int TryGetPriority(IBindingBuilder expression)
        {
            if (expression is IHasPriority hasPriority)
                return hasPriority.Priority;
            if (expression is IHasTargetExpressionBindingBuilder hasTargetExpression)
                return TryGetPriority(hasTargetExpression.TargetExpression);
            return 0;
        }

        private int TryGetPriority(IExpressionNode expression)
        {
            if (expression is IBindingMemberExpressionNode bindingMemberExpression && TryGetPriority(bindingMemberExpression.Path, out var priority))
                return priority;
            var node = expression.TryGetRootMemberExpression();
            if (node is IHasPriority hasPriority)
                return hasPriority.Priority;
            if (node is IMemberExpressionNode member && TryGetPriority(member.Member, out priority))
                return priority;
            return 0;
        }

        private bool TryGetPriority(string member, out int priority)
        {
            if (FakeMemberProvider.IsFakeMember(member))
            {
                priority = FakeMemberPriority;
                return true;
            }

            return BindingMemberPriorities.TryGetValue(member, out priority);
        }

        #endregion
    }
}