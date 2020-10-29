using System.Linq;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class InlineBindingExpressionInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldIgnoreEventContext()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);
            context.BindingComponents[BindingParameterNameConstant.EventHandler] = null;

            var initializer = new InlineBindingExpressionInitializer();
            initializer.Initialize(null!, context);
            context.BindingComponents.Single().Key.ShouldEqual(BindingParameterNameConstant.EventHandler);
        }

        [Fact]
        public void ShouldIgnoreBindingModeContext()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);
            context.BindingComponents[BindingParameterNameConstant.Mode] = null;

            var initializer = new InlineBindingExpressionInitializer();
            initializer.Initialize(null!, context);
            context.BindingComponents.Single().Key.ShouldEqual(BindingParameterNameConstant.Mode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIgnoreBindingMemberContext(bool isStatic)
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, new TestBindingMemberExpressionNode {MemberFlags = isStatic ? MemberFlags.StaticAll : MemberFlags.InstanceAll}, default, null);

            var initializer = new InlineBindingExpressionInitializer {UseOneTimeModeForStaticMembersImplicit = false};
            initializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSetOneTimeModeConstantExpression()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);

            var initializer = new InlineBindingExpressionInitializer();
            initializer.Initialize(null!, context);
            var pair = context.BindingComponents.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }

        [Fact]
        public void ShouldSetOneTimeModeStaticExpression()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, new TestBindingMemberExpressionNode {MemberFlags = MemberFlags.StaticAll}, default, null);

            var initializer = new InlineBindingExpressionInitializer {UseOneTimeModeForStaticMembersImplicit = true};
            initializer.Initialize(null!, context);
            var pair = context.BindingComponents.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }

        #endregion
    }
}