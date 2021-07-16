using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class DelayBindingInitializerTest : UnitTestBase
    {
        private readonly BindingExpressionInitializerContext _context;
        private readonly DelayBindingInitializer _initializer;

        public DelayBindingInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _initializer = new DelayBindingInitializer();
            _context = new BindingExpressionInitializerContext(this);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddDelayComponent(bool ignore)
        {
            const int delay = 100;
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Delay),
                ConstantExpressionNode.Get(delay));
            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, Metadata);
            if (ignore)
                _context.Components[BindingParameterNameConstant.Delay] = null;

            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(1);
            if (ignore)
            {
                _context.Components[BindingParameterNameConstant.Delay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider)_context.Components[BindingParameterNameConstant.Delay]!;
            var component = (DelayBindingHandler.Source)provider.TryGetComponent(null!, null!, null, Metadata)!;
            component.Delay.ShouldEqual((ushort)delay);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void InitializeShouldAddTargetDelayComponent(bool ignore)
        {
            const int delay = 100;
            var parameter = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.TargetDelay),
                ConstantExpressionNode.Get(delay));
            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, parameter, Metadata);
            if (ignore)
                _context.Components[BindingParameterNameConstant.TargetDelay] = null;

            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(1);
            if (ignore)
            {
                _context.Components[BindingParameterNameConstant.TargetDelay].ShouldBeNull();
                return;
            }

            var provider = (IBindingComponentProvider)_context.Components[BindingParameterNameConstant.TargetDelay]!;
            var component = (DelayBindingHandler.Target)provider.TryGetComponent(null!, null!, null, Metadata)!;
            component.Delay.ShouldEqual((ushort)delay);
        }

        [Fact]
        public void InitializeShouldIgnoreEmptyParameters()
        {
            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, Metadata);
            _initializer.Initialize(null!, _context);
            _context.Components.ShouldBeEmpty();
        }
    }
}