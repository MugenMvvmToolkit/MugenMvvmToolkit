using System;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    public class DelayBindingHandlerTest : UnitTestBase
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
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            };

            var component = (DelayBindingHandler.Source) DelayBindingHandler.GetSource(delay);
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
            const int wait = 10 * 4;
            Action? invokeAction = null;
            var testDispatcher = new TestThreadDispatcherComponent
            {
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            };

            var component = (DelayBindingHandler.Target) DelayBindingHandler.GetTarget(delay);
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