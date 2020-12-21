using System.Collections.Generic;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
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
            initializer.Initialize(null!, context);
            context.Components.ShouldBeEmpty();
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
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, ItemOrList.FromItem<IExpressionNode, IList<IExpressionNode>>(parameter), DefaultMetadata);
            if (ignore)
                context.Components[BindingParameterNameConstant.Delay] = null;

            initializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(1);
            if (ignore)
            {
                context.Components[BindingParameterNameConstant.Delay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider) context.Components[BindingParameterNameConstant.Delay]!;
            var component = (DelayBindingComponent.Source) provider.TryGetComponent(null!, null!, null, DefaultMetadata)!;
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
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, ItemOrList.FromItem<IExpressionNode, IList<IExpressionNode>>(parameter), DefaultMetadata);
            if (ignore)
                context.Components[BindingParameterNameConstant.TargetDelay] = null;

            initializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(1);
            if (ignore)
            {
                context.Components[BindingParameterNameConstant.TargetDelay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider) context.Components[BindingParameterNameConstant.TargetDelay]!;
            var component = (DelayBindingComponent.Target) provider.TryGetComponent(null!, null!, null, DefaultMetadata)!;
            component.Delay.ShouldEqual((ushort) delay);
        }

        #endregion
    }
}