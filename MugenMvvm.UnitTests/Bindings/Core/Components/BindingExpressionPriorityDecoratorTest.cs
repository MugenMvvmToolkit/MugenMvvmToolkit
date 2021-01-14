﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingExpressionPriorityDecoratorTest : UnitTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TryParseBindingExpressionShouldOrderList(int inputParameterState)
        {
            const string maxPriorityName = "T1";
            const string minPriorityName = "T2";
            const string fakePriorityName = FakeMemberProvider.FakeMemberPrefix + "2";

            var expressions = new IBindingBuilder[]
            {
                new NoPriorityBindingBuilder(),
                new TestBindingBuilder {TargetExpression = new MemberExpressionNode(new MemberExpressionNode(null, minPriorityName), Guid.NewGuid().ToString())},
                new TestBindingBuilder {TargetExpression = new TestBindingMemberExpressionNode(maxPriorityName)},
                new HasPriorityBindingBuilder {Priority = int.MaxValue - 1},
                new TestBindingBuilder {TargetExpression = new HasPriorityExpressionNode {Priority = 1}},
                new TestBindingBuilder {TargetExpression = new TestBindingMemberExpressionNode(fakePriorityName)}
            };
            var expected = new[] {expressions[2], expressions[3], expressions[5], expressions[4], expressions[0], expressions[1]};
            IList<IBindingBuilder> result;
            if (inputParameterState == 1)
                result = expressions;
            else if (inputParameterState == 2)
                result = expressions.ToList();
            else
                result = new Collection<IBindingBuilder>(expressions);

            var bindingManager = new BindingManager();
            var decorator = new BindingExpressionPriorityDecorator {FakeMemberPriority = 2};
            decorator.BindingMemberPriorities.Clear();
            decorator.BindingMemberPriorities[maxPriorityName] = int.MaxValue;
            decorator.BindingMemberPriorities[minPriorityName] = int.MinValue;
            bindingManager.AddComponent(decorator);
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, arg3) => ItemOrIReadOnlyList.FromRawValue<IBindingBuilder>(result)
            });

            var bindingExpressions = bindingManager.TryParseBindingExpression("", DefaultMetadata).AsList();
            bindingExpressions.ShouldEqual(expected);
        }

        private sealed class HasPriorityExpressionNode : ExpressionNodeBase<HasPriorityExpressionNode>, IHasPriority, IMemberExpressionNode
        {
            public HasPriorityExpressionNode(IReadOnlyDictionary<string, object?>? metadata = null) : base(metadata)
            {
            }

            public override ExpressionNodeType ExpressionType => ExpressionNodeType.Index;

            public int Priority { get; set; }

            public IExpressionNode? Target { get; } = null!;

            public string Member { get; } = null!;

            public IMemberExpressionNode UpdateTarget(IExpressionNode? target) => throw new NotSupportedException();

            protected override IExpressionNode Visit(IExpressionVisitor visitor, IReadOnlyMetadataContext? metadata) => throw new NotSupportedException();

            protected override HasPriorityExpressionNode Clone(IReadOnlyDictionary<string, object?> metadata) => throw new NotSupportedException();

            protected override bool Equals(HasPriorityExpressionNode other, IExpressionEqualityComparer? comparer) => throw new NotSupportedException();

            protected override int GetHashCode(int hashCode, IExpressionEqualityComparer? comparer) => throw new NotSupportedException();
        }

        private sealed class NoPriorityBindingBuilder : IBindingBuilder
        {
            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null) => throw new NotSupportedException();
        }

        private sealed class HasPriorityBindingBuilder : IHasPriority, IBindingBuilder
        {
            public int Priority { get; set; }

            public IBinding Build(object target, object? source = null, IReadOnlyMetadataContext? metadata = null) => throw new NotSupportedException();
        }

        [Fact]
        public void TryParseBindingExpressionShouldIgnoreNonListResult()
        {
            var request = "";
            var exp = new TestBindingBuilder();
            var bindingManager = new BindingManager();
            bindingManager.AddComponent(new BindingExpressionPriorityDecorator());
            bindingManager.AddComponent(new TestBindingExpressionParserComponent
            {
                TryParseBindingExpression = (o, arg3) =>
                {
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return exp;
                }
            });
            bindingManager.TryParseBindingExpression(request, DefaultMetadata).AsList().Single().ShouldEqual(exp);
        }
    }
}