using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Parsing.Expressions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class BindingModeInitializerTest : UnitTestBase
    {
        [Fact]
        public void InitializeShouldCheckModeInlineParameters()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in bindingModeInitializer.BindingModes)
            {
                context.Components.Clear();
                context.ParameterExpressions = new MemberExpressionNode(null, bindingMode.Key);
                bindingModeInitializer.Initialize(null!, context);
                if (bindingMode.Value == null)
                    context.Components.Count.ShouldEqual(0);
                else
                {
                    context.Components.Count.ShouldEqual(1);
                    context.Components[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldCheckModeParameters()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.BindingModes.ShouldNotBeNull();
            foreach (var bindingMode in bindingModeInitializer.BindingModes)
            {
                context.Components.Clear();
                context.ParameterExpressions = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode),
                    new MemberExpressionNode(null, bindingMode.Key));
                bindingModeInitializer.Initialize(null!, context);
                if (bindingMode.Value == null)
                    context.Components.Count.ShouldEqual(0);
                else
                {
                    context.Components.Count.ShouldEqual(1);
                    context.Components[BindingParameterNameConstant.Mode].ShouldEqual(bindingMode.Value);
                }
            }
        }

        [Fact]
        public void InitializeShouldIgnoreInitializedMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            context.Components[BindingParameterNameConstant.Mode] = this;
            bindingModeInitializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(1);
            context.Components[BindingParameterNameConstant.Mode].ShouldEqual(this);

            context.Components[BindingParameterNameConstant.Mode] = null;
            bindingModeInitializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void InitializeShouldThrowInvalidMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            context.ParameterExpressions = new BinaryExpressionNode(BinaryTokenType.Assignment, new MemberExpressionNode(null, BindingParameterNameConstant.Mode),
                new MemberExpressionNode(null, "Test"));
            ShouldThrow<InvalidOperationException>(() => bindingModeInitializer.Initialize(null!, context));
        }

        [Fact]
        public void InitializeShouldUseDefaultMode()
        {
            var bindingModeInitializer = new BindingModeInitializer();
            var context = new BindingExpressionInitializerContext(this);
            context.Initialize(this, this, MemberExpressionNode.Empty, MemberExpressionNode.Action, default, DefaultMetadata);

            bindingModeInitializer.DefaultMode = OneWayBindingMode.Instance;
            bindingModeInitializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(1);
            context.Components[BindingParameterNameConstant.Mode].ShouldEqual(bindingModeInitializer.DefaultMode);

            context.Components.Clear();
            bindingModeInitializer.DefaultMode = null;
            bindingModeInitializer.Initialize(null!, context);
            context.Components.Count.ShouldEqual(0);
        }
    }
}