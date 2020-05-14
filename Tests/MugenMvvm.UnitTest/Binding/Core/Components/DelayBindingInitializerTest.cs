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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddDelayComponent(bool ignore)
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Delay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.Delay] = null;
            }

            initializer.Initialize(context);
            context.BindingComponents.Count.ShouldEqual(1);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.Delay].ShouldBeNull();
                return;
            }
            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.Delay]!;
            var component = (DelayBindingComponent.Source) provider.GetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddTargetDelayComponent(bool ignore)
        {
            const int delay = 100;
            var initializer = new DelayBindingInitializer();
            var context = new BindingExpressionInitializerContext(this);
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay), ConstantExpressionNode.Get(delay));
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, DefaultMetadata);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.TargetDelay] = null;
            }

            initializer.Initialize(context);
            if (ignore)
            {
                context.BindingComponents[BindingParameterNameConstant.TargetDelay].ShouldBeNull();
                return;
            }
            context.BindingComponents.Count.ShouldEqual(1);
            var provider = (IBindingComponentProvider) context.BindingComponents[BindingParameterNameConstant.TargetDelay]!;
            var component = (DelayBindingComponent.Target) provider.GetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        #endregion
    }
}