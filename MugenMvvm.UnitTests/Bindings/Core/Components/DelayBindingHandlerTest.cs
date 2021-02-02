using System;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Threading;
using MugenMvvm.UnitTests.Bindings.Core.Internal;
using MugenMvvm.UnitTests.Threading.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Core.Components
{
    [Collection(SharedContext)]
    public class DelayBindingHandlerTest : UnitTestBase, IDisposable
    {
        private readonly TestBinding _binding;

        public DelayBindingHandlerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _binding = new TestBinding(ComponentCollectionManager);
            MugenService.Configuration.InitializeInstance<IThreadDispatcher>(new ThreadDispatcher(ComponentCollectionManager));
        }

        public void Dispose() => MugenService.Configuration.Clear<IThreadDispatcher>();

        [Fact]
        public void ShouldDelaySourceValue()
        {
            const int delay = 10;
            const int wait = 10 * 3;
            Action? invokeAction = null;

            var component = (DelayBindingHandler.Source) DelayBindingHandler.GetSource(delay);
            var sourceUpdateCount = 0;
            MugenService.AddComponent(new TestThreadDispatcherComponent
            {
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            });
            _binding.UpdateTarget = () => throw new NotSupportedException();
            _binding.UpdateSource = () =>
            {
                component.InterceptSourceValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                ++sourceUpdateCount;
            };
            component.Delay.ShouldEqual((ushort) delay);
            ((IAttachableComponent) component).OnAttached(_binding, DefaultMetadata);

            component.InterceptSourceValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptSourceValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptSourceValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent) component).OnDetached(_binding, DefaultMetadata);
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

            var component = (DelayBindingHandler.Target) DelayBindingHandler.GetTarget(delay);
            var sourceUpdateCount = 0;
            MugenService.AddComponent(new TestThreadDispatcherComponent
            {
                Execute = (action, mode, arg3, arg4) =>
                {
                    invokeAction.ShouldBeNull();
                    mode.ShouldEqual(DelayBindingHandler.ExecutionMode);
                    invokeAction = () => action(arg3);
                    return true;
                }
            });
            _binding.UpdateSource = () => throw new NotSupportedException();
            _binding.UpdateTarget = () =>
            {
                component.InterceptTargetValue(null!, default, this, DefaultMetadata).ShouldEqual(this);
                ++sourceUpdateCount;
            };
            component.Delay.ShouldEqual((ushort) delay);
            ((IAttachableComponent) component).OnAttached(_binding, DefaultMetadata);

            component.InterceptTargetValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            component.InterceptTargetValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldNotBeNull();
            sourceUpdateCount.ShouldEqual(0);
            invokeAction!();
            sourceUpdateCount.ShouldEqual(1);

            invokeAction = null;
            sourceUpdateCount = 0;
            component.InterceptTargetValue(_binding, default, null, DefaultMetadata).IsDoNothing().ShouldBeTrue();
            ((IDetachableComponent) component).OnDetached(_binding, DefaultMetadata);
            invokeAction.ShouldBeNull();
            WaitCompletion(wait);
            invokeAction.ShouldBeNull();
        }
    }
}