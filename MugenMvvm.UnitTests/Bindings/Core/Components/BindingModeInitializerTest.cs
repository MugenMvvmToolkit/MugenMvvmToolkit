using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingModeInitializerTest : UnitTestBase
    {
        private readonly BindingModeInitializer _initializer;
        private readonly BindingExpressionInitializerContext _context;

        public BindingModeInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _initializer = new BindingModeInitializer();
            _context = new BindingExpressionInitializerContext(this);
            _context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);
        }

        [Fact]
        public void InitializeShouldCheckModeInlineParameters()
        {
            _initializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in _initializer.BindingModes)
            {
                _context.Components.Clear();
                _context.ParameterExpressions = new MemberExpressionNode(null, bindingMode.Key);
                _initializer.Initialize(null!, _context);
                if (bindingMode.Value == null)
                    _context.Components.Count.ShouldEqual(0);
                else
                {
                    _context.Components.Count.ShouldEqual(1);
                    _context.Components[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldCheckModeParameters()
        {
            _initializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in _initializer.BindingModes)
            {
                _context.Components.Clear();
                _context.ParameterExpressions = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode),
                    new MemberExpressionNode(null, bindingMode.Key));
                _initializer.Initialize(null!, _context);
                if (bindingMode.Value == null)
                    _context.Components.Count.ShouldEqual(0);
                else
                {
                    _context.Components.Count.ShouldEqual(1);
                    _context.Components[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldIgnoreInitializedMode()
        {
            _context.Components[BindingParameterNameConstant.Mode] = this;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(1);
            _context.Components[BindingParameterNameConstant.Mode].ShouldEqual(this);

            _context.Components[BindingParameterNameConstant.Mode] = null;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void InitializeShouldThrowInvalidMode()
        {
            _context.ParameterExpressions = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode),
                new MemberExpressionNode(null, "Test"));
            ShouldThrow<InvalidOperationException>(() => _initializer.Initialize(null!, _context));
        }

        [Fact]
        public void InitializeShouldUseDefaultMode()
        {
            _initializer.DefaultMode = OneWayBindingMode.Instance;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(1);
            _context.Components[BindingParameterNameConstant.Mode].ShouldEqual(_initializer.DefaultMode);

            _context.Components.Clear();
            _initializer.DefaultMode = null;
            _initializer.Initialize(null!, _context);
            _context.Components.Count.ShouldEqual(0);
        }
    }
}