using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Tests.Bindings.Parsing;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Parsing.Expressions
{
    public class MemberExpressionNodeTest : UnitTestBase
    {
        private const string MemberName = "@4";

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldCreateNewNode1(int value)
        {
            var target = new ConstantExpressionNode("1");
            var targetChanged = new ConstantExpressionNode("1-");
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    if (node == target)
                        return targetChanged;
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };
            var exp = new MemberExpressionNode(target, MemberName);
            var expressionNode = (MemberExpressionNode)exp.Accept(visitor, Metadata);
            expressionNode.ShouldNotEqual(exp);
            expressionNode.Target.ShouldEqual(targetChanged);
            expressionNode.Member.ShouldEqual(MemberName);
        }

        [Fact]
        public void AcceptShouldCreateNewNode2()
        {
            var target = new ConstantExpressionNode("1");
            var testExpressionVisitor = new TestExpressionVisitor
            {
                Visit = (node, context) => target
            };
            new MemberExpressionNode(target, MemberName).Accept(testExpressionVisitor, Metadata).ShouldEqual(target);
        }

        [Theory]
        [InlineData(ExpressionTraversalType.InorderValue)]
        [InlineData(ExpressionTraversalType.PreorderValue)]
        [InlineData(ExpressionTraversalType.PostorderValue)]
        public void AcceptShouldVisitWithCorrectOrder(int value)
        {
            var nodes = new List<IExpressionNode>();
            var visitor = new TestExpressionVisitor
            {
                Visit = (node, context) =>
                {
                    nodes.Add(node);
                    context.ShouldEqual(Metadata);
                    return node;
                },
                TraversalType = ExpressionTraversalType.Get(value)
            };

            var target = new ConstantExpressionNode("1");
            var exp = new MemberExpressionNode(target, MemberName);

            var result = visitor.TraversalType == ExpressionTraversalType.Preorder ? new IExpressionNode[] { exp, target } : new IExpressionNode[] { target, exp };
            exp.Accept(visitor, Metadata).ShouldEqual(exp);
            result.ShouldEqual(nodes);
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new ConstantExpressionNode("1");
            var exp = new MemberExpressionNode(target, MemberName);
            exp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            exp.Target.ShouldEqual(target);
            exp.Member.ShouldEqual(MemberName);
            exp.ToString().ShouldEqual("\"1\".@4");
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void GetHashCodeEqualsShouldBeValid(bool withComparer, bool hasTarget)
        {
            var comparer = withComparer ? new TestExpressionEqualityComparer() : null;
            var exp1 = new MemberExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null, "Member", new Dictionary<string, object?> { { "k", null } });
            var exp2 = new MemberExpressionNode(hasTarget ? GetTestEqualityExpression(comparer, 1) : null, "Member", new Dictionary<string, object?> { { "k", null } });
            if (hasTarget)
            {
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Member, 1).ShouldEqual(exp1.GetHashCode(comparer));
                ((TestExpressionNode)exp1.Target!).GetHashCodeCount.ShouldEqual(1);
            }
            else
                HashCode.Combine(GetBaseHashCode(exp1), exp1.Member).ShouldEqual(exp1.GetHashCode(comparer));

            exp1.Equals(exp2, comparer).ShouldBeTrue();
            ((TestExpressionNode?)exp1.Target)?.EqualsCount.ShouldEqual(1);

            exp1.Equals(exp2.UpdateMetadata(null), comparer).ShouldBeFalse();
            ((TestExpressionNode?)exp1.Target)?.EqualsCount.ShouldEqual(1);

            if (comparer == null || !hasTarget)
                return;
            comparer.GetHashCode = node =>
            {
                ReferenceEquals(node, exp1).ShouldBeTrue();
                return int.MaxValue;
            };
            comparer.Equals = (x1, x2) =>
            {
                ReferenceEquals(x1, exp1).ShouldBeTrue();
                ReferenceEquals(x2, exp2).ShouldBeTrue();
                return false;
            };
            exp1.GetHashCode(comparer).ShouldEqual(int.MaxValue);
            exp1.Equals(exp2, comparer).ShouldBeFalse();
            ((TestExpressionNode)exp1.Target!).EqualsCount.ShouldEqual(1);
        }

        [Fact]
        public void GetShouldReturnCachedMembers()
        {
            MemberExpressionNode.Get(null, MacrosConstant.Self).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.This).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.Target).ShouldEqual(MemberExpressionNode.Self);
            MemberExpressionNode.Get(null, MacrosConstant.Context).ShouldEqual(MemberExpressionNode.Context);
            MemberExpressionNode.Get(null, MacrosConstant.Source).ShouldEqual(MemberExpressionNode.Source);
            MemberExpressionNode.Get(null, MacrosConstant.EventArgs).ShouldEqual(MemberExpressionNode.EventArgs);
            MemberExpressionNode.Get(null, MacrosConstant.Binding).ShouldEqual(MemberExpressionNode.Binding);
            MemberExpressionNode.Get(null, MacrosConstant.Action).ShouldEqual(MemberExpressionNode.Action);
            MemberExpressionNode.Get(null, "").ShouldEqual(MemberExpressionNode.Empty);

            MemberExpressionNode.Get(null, BindingModeNameConstant.None).ShouldEqual(MemberExpressionNode.NoneMode);
            MemberExpressionNode.Get(null, BindingModeNameConstant.OneTime).ShouldEqual(MemberExpressionNode.OneTimeMode);
            MemberExpressionNode.Get(null, BindingModeNameConstant.OneWay).ShouldEqual(MemberExpressionNode.OneWayMode);
            MemberExpressionNode.Get(null, BindingModeNameConstant.OneWayToSource).ShouldEqual(MemberExpressionNode.OneWayToSourceMode);
            MemberExpressionNode.Get(null, BindingModeNameConstant.TwoWay).ShouldEqual(MemberExpressionNode.TwoWayMode);

            MemberExpressionNode.Get(null, BindingParameterNameConstant.Optional).ShouldEqual(MemberExpressionNode.OptionalParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.HasStablePath).ShouldEqual(MemberExpressionNode.HasStablePathParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.Observable).ShouldEqual(MemberExpressionNode.ObservableParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.ToggleEnabled).ShouldEqual(MemberExpressionNode.ToggleEnabledParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.SuppressMethodAccessors).ShouldEqual(MemberExpressionNode.SuppressMethodAccessorsParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.SuppressIndexAccessors).ShouldEqual(MemberExpressionNode.SuppressIndexAccessorsParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.ObservableMethods).ShouldEqual(MemberExpressionNode.ObservableMethodsParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.Converter).ShouldEqual(MemberExpressionNode.ConverterParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.ConverterParameter).ShouldEqual(MemberExpressionNode.ConverterParameterParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.Fallback).ShouldEqual(MemberExpressionNode.FallbackParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.TargetNullValue).ShouldEqual(MemberExpressionNode.TargetNullValueParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.CommandParameter).ShouldEqual(MemberExpressionNode.CommandParameterParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.Delay).ShouldEqual(MemberExpressionNode.DelayParameter);
            MemberExpressionNode.Get(null, BindingParameterNameConstant.TargetDelay).ShouldEqual(MemberExpressionNode.TargetDelayParameter);


            var node = MemberExpressionNode.Get(ConstantExpressionNode.False, MacrosConstant.Self);
            node.ShouldNotEqual(MemberExpressionNode.Self);
            node.Target.ShouldEqual(ConstantExpressionNode.False);
            node.Member.ShouldEqual(MacrosConstant.Self);
        }

        [Fact]
        public void StaticFieldsShouldBeCorrect()
        {
            MemberExpressionNode.Empty.Target.ShouldBeNull();
            MemberExpressionNode.Empty.Member.ShouldEqual(string.Empty);

            MemberExpressionNode.Source.Target.ShouldBeNull();
            MemberExpressionNode.Source.Member.ShouldEqual(MacrosConstant.Source);

            MemberExpressionNode.Self.Target.ShouldBeNull();
            MemberExpressionNode.Self.Member.ShouldEqual(MacrosConstant.Target);

            MemberExpressionNode.Context.Target.ShouldBeNull();
            MemberExpressionNode.Context.Member.ShouldEqual(MacrosConstant.Context);

            MemberExpressionNode.Binding.Target.ShouldBeNull();
            MemberExpressionNode.Binding.Member.ShouldEqual(MacrosConstant.Binding);

            MemberExpressionNode.EventArgs.Target.ShouldBeNull();
            MemberExpressionNode.EventArgs.Member.ShouldEqual(MacrosConstant.EventArgs);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UpdateMetadataShouldCheckMetadataEquality(bool equal)
        {
            var node = new MemberExpressionNode(null, string.Empty, EmptyDictionary);
            if (equal)
                node.UpdateMetadata(EmptyDictionary).ShouldEqual(node, ReferenceEqualityComparer.Instance);
            else
            {
                var metadata = new Dictionary<string, object?> { { "k", null } };
                var updated = (MemberExpressionNode)node.UpdateMetadata(metadata);
                updated.ShouldNotEqual(node, ReferenceEqualityComparer.Instance);
                updated.Metadata.ShouldEqual(metadata);
                updated.Member.ShouldEqual(node.Member);
                updated.Target.ShouldEqual(node.Target);
            }
        }

        [Fact]
        public void UpdateTargetShouldCreateNewNode()
        {
            var target = new ConstantExpressionNode("1");
            var newTarget = new ConstantExpressionNode("2");
            var exp = new MemberExpressionNode(target, MemberName);
            exp.UpdateTarget(target).ShouldEqual(exp);

            var newExp = exp.UpdateTarget(newTarget);
            newExp.ShouldNotEqual(exp);
            newExp.ExpressionType.ShouldEqual(ExpressionNodeType.Member);
            newExp.Target.ShouldEqual(newTarget);
            newExp.Member.ShouldEqual(MemberName);
        }
    }
}