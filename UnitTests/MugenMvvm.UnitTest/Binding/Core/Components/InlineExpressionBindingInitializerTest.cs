using System.Linq;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.UnitTest.Binding.Parsing.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Core.Components
{
    public class InlineExpressionBindingInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldIgnoreEventContext()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);
            context.BindingComponents[BindingParameterNameConstant.EventHandler] = null;

            var initializer = new InlineExpressionBindingInitializer();
            initializer.Initialize(null!, context);
            context.BindingComponents.Single().Key.ShouldEqual(BindingParameterNameConstant.EventHandler);
        }

        [Fact]
        public void ShouldIgnoreBindingModeContext()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);
            context.BindingComponents[BindingParameterNameConstant.Mode] = null;

            var initializer = new InlineExpressionBindingInitializer();
            initializer.Initialize(null!, context);
            context.BindingComponents.Single().Key.ShouldEqual(BindingParameterNameConstant.Mode);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIgnoreBindingMemberContext(bool isStatic)
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, new TestBindingMemberExpressionNode { MemberFlags = isStatic ? MemberFlags.StaticAll : MemberFlags.InstanceAll }, default, null);

            var initializer = new InlineExpressionBindingInitializer { UseOneTimeModeForStaticMembersImplicit = false };
            initializer.Initialize(null!, context);
            context.BindingComponents.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSetOneTimeModeConstantExpression()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);

            var initializer = new InlineExpressionBindingInitializer();
            initializer.Initialize(null!, context);
            var pair = context.BindingComponents.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }

        [Fact]
        public void ShouldSetOneTimeModeStaticExpression()
        {
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, new TestBindingMemberExpressionNode { MemberFlags = MemberFlags.StaticAll }, default, null);

            var initializer = new InlineExpressionBindingInitializer { UseOneTimeModeForStaticMembersImplicit = true };
            initializer.Initialize(null!, context);
            var pair = context.BindingComponents.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }

        #endregion
    }
}