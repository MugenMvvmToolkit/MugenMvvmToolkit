using System;
using MugenMvvm.Binding.Core.Components.Binding;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Binding.Core.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Core.Components.Binding
{
    public class DelayBindingComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldDelaySourceValue()
        {
            const int delay = 10;
            const int wait = 10 * 3;
            Action? invokeAction = null;
            var testDispatcher = new TestThreadDispatcherComponent
            {
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingComponent.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            };

            var component = (DelayBindingComponent.Source) DelayBindingComponent.GetSource(delay);
            var sourceUpdateCount = 0;
            using var t = MugenService.AddComponent(testDispatcher);
            var testBinding = new TestBinding
            {
                UpdateTarget = () => throw new NotSupportedException(),
                UpdateSource = () =>
                {
                    component.InterceptSourceValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                    ++sourceUpdateCount;
                }
            };
            component.Delay.ShouldEqual((ushort) delay);
            ((IAttachableComponent) component).OnAttached(testBinding, DefaultMetadata);

            component.InterceptSourceValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptSourceValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent) component).OnDetached(testBinding, DefaultMetadata);
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldBeNull();
        }

        [Fact]
        public void ShouldDelayTargetValue()
        {
            const int delay = 10;
            const int wait = 10 * 3;
            Action? invokeAction = null;
            var testDispatcher = new TestThreadDispatcherComponent
            {
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingComponent.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            };

            var component = (DelayBindingComponent.Target) DelayBindingComponent.GetTarget(delay);
            var sourceUpdateCount = 0;
            using var t = MugenService.AddComponent(testDispatcher);
            var testBinding = new TestBinding
            {
                UpdateSource = () => throw new NotSupportedException(),
                UpdateTarget = () =>
                {
                    component.InterceptTargetValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                    ++sourceUpdateCount;
                }
            };
            component.Delay.ShouldEqual((ushort) delay);
            ((IAttachableComponent) component).OnAttached(testBinding, DefaultMetadata);

            component.InterceptTargetValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptTargetValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(testBinding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent) component).OnDetached(testBinding, DefaultMetadata);
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldBeNull();
        }

        #endregion
    }
}