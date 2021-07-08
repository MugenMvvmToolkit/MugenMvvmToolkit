﻿using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingExpressionPriorityDecorator : ComponentDecoratorBase<IBindingManager, IBindingExpressionParserComponent>, IBindingExpressionParserComponent,
        IComparer<IBindingBuilder>
    {
        [Preserve(Conditional = true)]
        public BindingExpressionPriorityDecorator(int priority = BindingComponentPriority.BuilderPriorityDecorator) : base(priority)
        {
            BindingMemberPriorities = new Dictionary<string, int>
            {
                { nameof(BindableMembers.DataContext), BindableMemberPriority.DataContext },
                { BindingInternalConstant.ItemTemplate, BindableMemberPriority.Template },
                { BindingInternalConstant.ItemTemplateSelector, BindableMemberPriority.Template },
                { BindingInternalConstant.ContentTemplate, BindableMemberPriority.Template },
                { BindingInternalConstant.ContentTemplateSelector, BindableMemberPriority.Template },
                { BindingInternalConstant.StableIdProvider, BindableMemberPriority.Template },
                { BindingInternalConstant.ItemsSource, BindableMemberPriority.ItemsSource }
            };
        }

        public Dictionary<string, int> BindingMemberPriorities { get; }

        public int FakeMemberPriority { get; set; } = BindableMemberPriority.Fake;

        public ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            var expressions = Components.TryParseBindingExpression(bindingManager, expression, metadata);
            if (expressions.Item != null || expressions.List == null)
                return expressions;

            if (expressions.List is IBindingBuilder[] array)
                Array.Sort(array, this);
            else if (expressions.List is List<IBindingBuilder> l)
                l.Sort(this);
            else
            {
                var result = expressions.List.ToArray();
                Array.Sort(result, this);
                return result;
            }

            return expressions;
        }

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

        int IComparer<IBindingBuilder>.Compare(IBindingBuilder? x, IBindingBuilder? y) => TryGetPriority(y!).CompareTo(TryGetPriority(x!));
    }
}