using System.Linq;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Expressions;
using MugenMvvm.UnitTests.Bindings.Parsing.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class InlineBindingExpressionInitializerTest : UnitTestBase
    {
        private readonly BindingExpressionInitializerContext _context;
        private readonly InlineBindingExpressionInitializer _initializer;
        private readonly TestBindingMemberExpressionNode _target;

        public InlineBindingExpressionInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _target = new TestBindingMemberExpressionNode("Test");
            _context = new BindingExpressionInitializerContext(this);
            _initializer = new InlineBindingExpressionInitializer();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldIgnoreBindingMemberContext(bool isStatic)
        {
            _context.Initialize(this, this, _target, new TestBindingMemberExpressionNode { MemberFlags = isStatic ? MemberFlags.StaticAll : MemberFlags.InstanceAll }, default,
                null);

            _initializer.UseOneTimeModeForStaticMembersImplicit = false;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldIgnoreBindingModeContext()
        {
            _context.Initialize(this, this, _target, MemberExpressionNode.Source, default, null);
            _context.Components[BindingParameterNameConstant.Mode] = null;

            _initializer.Initialize(null!, _context);
            _context.Components.Single().Key.ShouldEqual(BindingParameterNameConstant.Mode);
        }

        [Fact]
        public void ShouldIgnoreEventContext()
        {
            _context.Initialize(this, this, _target, MemberExpressionNode.Source, default, null);
            _context.Components[BindingParameterNameConstant.EventHandler] = null;

            _initializer.Initialize(null!, _context);
            _context.Components.Single().Key.ShouldEqual(BindingParameterNameConstant.EventHandler);
        }

        [Fact]
        public void ShouldIgnoreMultiPathTarget()
        {
            _context.Initialize(this, this, new TestBindingMemberExpressionNode("Test.Test"), MemberExpressionNode.Source, default, null);

            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldIgnoreNonBidingMemberTarget()
        {
            _context.Initialize(this, this, MemberExpressionNode.TargetNullValueParameter, MemberExpressionNode.Source, default, null);

            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldSetOneTimeModeConstantExpression()
        {
            _context.Initialize(this, this, _target, MemberExpressionNode.Source, default, null);

            _initializer.Initialize(null!, _context);
            var pair = _context.Components.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }

        [Fact]
        public void ShouldSetOneTimeModeStaticExpression()
        {
            _context.Initialize(this, this, _target, new TestBindingMemberExpressionNode { MemberFlags = MemberFlags.StaticAll }, default, null);

            _initializer.UseOneTimeModeForStaticMembersImplicit = true;
            _initializer.Initialize(null!, _context);
            var pair = _context.Components.Single();
            pair.Key.ShouldEqual(BindingParameterNameConstant.Mode);
            pair.Value.ShouldEqual(OneTimeBindingMode.Instance);
        }
    }
}