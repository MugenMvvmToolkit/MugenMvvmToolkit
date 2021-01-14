using System;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core
{
    public class BindingExpressionInitializerContextTest : MetadataOwnerTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ParametersShouldInitializeValues(bool reinitialize)
        {
            const string parameter1 = "p1";
            const string parameter2 = "p2";
            const string parameter3 = "p3";
            const string parameter4 = "p4";

            var parameterValue1 = new object();
            var parameterValue2 = false;
            var parameterValue3 = true;
            var parameterValue4 = true;

            var context = new BindingExpressionInitializerContext(this);
            var items = new IExpressionNode[]
            {
                new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, parameter1), ConstantExpressionNode.Get(parameterValue1)),
                new UnaryExpressionNode(UnaryTokenType.LogicalNegation, new MemberExpressionNode(null, parameter2)),
                new MemberExpressionNode(null, parameter3)
            };
            context.Initialize(this, this, MemberExpressionNode.Self, MemberExpressionNode.Source, items, null);

            context.AssignmentParameters.Count.ShouldEqual(1);
            context.AssignmentParameters[parameter1].ShouldEqual(ConstantExpressionNode.Get(parameterValue1));
            context.TryGetParameterValue<IExpressionNode>(parameter1).ShouldEqual(ConstantExpressionNode.Get(parameterValue1));

            context.InlineParameters.Count.ShouldEqual(2);
            context.InlineParameters[parameter2].ShouldEqual(parameterValue2);
            context.TryGetParameterValue<bool>(parameter2).ShouldEqual(parameterValue2);
            context.TryGetParameterValue<bool?>(parameter2).ShouldEqual(parameterValue2);

            context.InlineParameters[parameter3].ShouldEqual(parameterValue3);
            context.TryGetParameterValue<bool>(parameter3).ShouldEqual(parameterValue3);
            context.TryGetParameterValue<bool?>(parameter3).ShouldEqual(parameterValue3);

            if (reinitialize)
                context.Initialize(this, this, MemberExpressionNode.Self, MemberExpressionNode.Source, new[] {new MemberExpressionNode(null, parameter4)}, null);
            else
                context.ParameterExpressions = new[] {new MemberExpressionNode(null, parameter4)};

            context.AssignmentParameters.Count.ShouldEqual(0);
            context.TryGetParameterValue<IExpressionNode>(parameter1).ShouldBeNull();

            context.InlineParameters.Count.ShouldEqual(1);
            context.InlineParameters.ContainsKey(parameter2).ShouldBeFalse();
            context.TryGetParameterValue<bool>(parameter2).ShouldBeFalse();
            context.TryGetParameterValue<bool?>(parameter2).ShouldBeNull();

            context.InlineParameters.ContainsKey(parameter3).ShouldBeFalse();
            context.TryGetParameterValue<bool>(parameter3).ShouldBeFalse();
            context.TryGetParameterValue<bool?>(parameter3).ShouldBeNull();

            context.InlineParameters[parameter4].ShouldEqual(parameterValue4);
            context.TryGetParameterValue<bool>(parameter4).ShouldEqual(parameterValue4);
            context.TryGetParameterValue<bool?>(parameter4).ShouldEqual(parameterValue4);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Empty, default, metadata);
            return context;
        }

        [Fact]
        public void InitializeClearShouldInitializeClearValues()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Owner.ShouldEqual(this);
            context.Components.ShouldBeEmpty();
            context.AssignmentParameters.ShouldBeEmpty();
            context.InlineParameters.ShouldBeEmpty();

            var target = new object();
            var source = new object();
            var targetExp = MemberExpressionNode.Self;
            var sourceExp = MemberExpressionNode.Source;
            var parameters = new[] {MemberExpressionNode.Self, MemberExpressionNode.Source};
            context.Initialize(target, source, targetExp, sourceExp, parameters, DefaultMetadata);

            context.Target.ShouldEqual(target);
            context.Source.ShouldEqual(source);
            context.TargetExpression.ShouldEqual(targetExp);
            context.SourceExpression.ShouldEqual(sourceExp);
            context.ParameterExpressions.ShouldEqual(parameters);

            context.Clear();
            context.Owner.ShouldEqual(this);
            context.Components.ShouldBeEmpty();
            context.AssignmentParameters.ShouldBeEmpty();
            context.InlineParameters.ShouldBeEmpty();
            context.Target.ShouldBeNull();
            context.Source.ShouldBeNull();
            context.TargetExpression.ShouldBeNull();
            context.SourceExpression.ShouldBeNull();
            context.ParameterExpressions.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetParameterValueShouldReturnCorrectValues()
        {
            const string parameter1 = "p1";
            var context = new BindingExpressionInitializerContext(this);

            context.TryGetParameterValue(parameter1, int.MaxValue).ShouldEqual(int.MaxValue);

            context.AssignmentParameters[parameter1] = ConstantExpressionNode.Get(1);
            context.TryGetParameterValue<int>(parameter1).ShouldEqual(1);

            context.AssignmentParameters[parameter1] = ConstantExpressionNode.Get(parameter1);
            context.TryGetParameterValue<string>(parameter1).ShouldEqual(parameter1);

            context.AssignmentParameters[parameter1] = new MemberExpressionNode(null, parameter1);
            context.TryGetParameterValue<string>(parameter1).ShouldEqual(parameter1);

            context.AssignmentParameters[parameter1] = new MemberExpressionNode(null, parameter1);
            ShouldThrow<InvalidOperationException>(() => context.TryGetParameterValue<int>(parameter1));
        }
    }
}