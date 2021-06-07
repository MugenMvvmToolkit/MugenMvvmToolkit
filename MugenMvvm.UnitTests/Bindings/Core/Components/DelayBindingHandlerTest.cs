using System;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    [Collection(SharedContext)]
    public class DelayBindingHandlerTest : UnitTestBase
    {
        public DelayBindingHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ThreadDispatcher));
        }

        [Fact]
        public void ShouldDelaySourceValue()
        {
            const int delay = 10;
            const int wait = 10 * 3;
            Action? invokeAction = null;

            var component = (DelayBindingHandler.Source)DelayBindingHandler.GetSource(delay);
            var sourceUpdateCount = 0;
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                Execute = (_, action, mode, arg3, _) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            });
            Binding.UpdateTarget = () => throw new NotSupportedException();
            Binding.UpdateSource = () =>
            {
                component.InterceptSourceValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                ++sourceUpdateCount;
            };
            component.Delay.ShouldEqual((ushort)delay);
            ((IAttachableComponent)component).OnAttached(Binding, DefaultMetadata);

            component.InterceptSourceValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptSourceValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent)component).OnDetached(Binding, DefaultMetadata);
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

            var component = (DelayBindingHandler.Target)DelayBindingHandler.GetTarget(delay);
            var sourceUpdateCount = 0;
            ThreadDispatcher.AddComponent(new TestThreadDispatcherComponent
            {
                Execute = (_, action, mode, arg3, _) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            });
            Binding.UpdateSource = () => throw new NotSupportedException();
            Binding.UpdateTarget = () =>
            {
                component.InterceptTargetValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                ++sourceUpdateCount;
            };
            component.Delay.ShouldEqual((ushort)delay);
            ((IAttachableComponent)component).OnAttached(Binding, DefaultMetadata);

            component.InterceptTargetValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptTargetValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(Binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent)component).OnDetached(Binding, DefaultMetadata);
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldBeNull();
        }

        protected override IThreadDispatcher GetThreadDispatcher() => new ThreadDispatcher(ComponentCollectionManager);
    }
}