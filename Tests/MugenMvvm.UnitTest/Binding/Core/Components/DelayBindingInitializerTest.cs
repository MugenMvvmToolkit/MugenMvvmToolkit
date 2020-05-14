using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class DelayBindingInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InitializeShouldIgnoreEmptyParameters()
        {
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);
            initializer.Initialize(context);
            context.BindingComponents.ShouldBeEmpty();
        }

        [Fact]
        public void InitializeShouldAddDelayComponent()
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Delay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            initializer.Initialize(context);
            context.BindingComponents.Count.ShouldEqual(1);
            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.Delay]!;
            var component = (DelayBindingComponent.Source) provider.GetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        [Fact]
        public void InitializeShouldAddTargetDelayComponent()
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            initializer.Initialize(context);
            context.BindingComponents.Count.ShouldEqual(1);
            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.TargetDelay]!;
            var component = (DelayBindingComponent.Target) provider.GetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        #endregion
    }
}