using System;
using System.Collections.Generic;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Tests.App;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.App
{
    public class MugenApplicationTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            Application.Metadata.ShouldNotBeNull();
            Application.HasMetadata.ShouldBeFalse();
            Application.Components.ShouldNotBeNull();
            Application.HasComponents.ShouldBeFalse();
            ShouldThrow<InvalidOperationException>(() =>
            {
                var _ = Application.PlatformInfo;
            });
        }

        [Fact]
        public void InitializeShouldBeHandledByComponent()
        {
            var flags = ApplicationFlags.DesignMode;
            var state = this;
            var states = new List<ApplicationLifecycleState>();
            var device = PlatformInfo.UnitTest;
            using var app = Application.AddComponent(new TestApplicationLifecycleListener
            {
                OnLifecycleChanged = (app, viewModelLifecycleState, st, metadata) =>
                {
                    app.ShouldEqual(Application);
                    states.Add(viewModelLifecycleState);
                    st.ShouldEqual(state);
                    metadata.ShouldEqual(DefaultMetadata);
                }
            });
            Application.Initialize(device, state, flags, DefaultMetadata);
            Application.PlatformInfo.ShouldEqual(device);
            Application.Flags.ShouldEqual(flags | ApplicationFlags.Initialized);
            states.Count.ShouldEqual(2);
            states[0].ShouldEqual(ApplicationLifecycleState.Initializing);
            states[1].ShouldEqual(ApplicationLifecycleState.Initialized);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void IsInStateShouldBeHandledByComponents(int componentCount)
        {
            var count = 0;
            var state = ApplicationLifecycleState.Activated;

            Application.IsInState(state, DefaultMetadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                var component = new TestLifecycleTrackerComponent<ApplicationLifecycleState>
                {
                    IsInState = (o, t, s, m) =>
                    {
                        t.ShouldEqual(Application);
                        s.ShouldEqual(state);
                        m.ShouldEqual(DefaultMetadata);
                        o.ShouldEqual(Application);
                        ++count;
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                Application.Components.TryAdd(component);
            }

            Application.IsInState(state, DefaultMetadata).ShouldBeFalse();
            count.ShouldEqual(componentCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var state = "state";
            var lifecycleState = ApplicationLifecycleState.Initialized;
            for (var i = 0; i < count; i++)
            {
                var component = new TestApplicationLifecycleListener
                {
                    OnLifecycleChanged = (app, viewModelLifecycleState, st, metadata) =>
                    {
                        app.ShouldEqual(Application);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                        ++invokeCount;
                    },
                    Priority = i
                };
                Application.AddComponent(component);
            }

            Application.OnLifecycleChanged(lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnUnhandledExceptionShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var type = UnhandledExceptionType.System;
            var ex = new Exception();
            for (var i = 0; i < count; i++)
            {
                var component = new TestUnhandledExceptionHandlerComponent
                {
                    OnUnhandledException = (app, e, t, metadata) =>
                    {
                        app.ShouldEqual(Application);
                        e.ShouldEqual(ex);
                        t.ShouldEqual(type);
                        metadata.ShouldEqual(DefaultMetadata);
                        ++invokeCount;
                    },
                    Priority = i
                };
                Application.AddComponent(component);
            }

            Application.OnUnhandledException(ex, type, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        protected override IMugenApplication GetApplication() => new MugenApplication(null, ComponentCollectionManager);
    }
}