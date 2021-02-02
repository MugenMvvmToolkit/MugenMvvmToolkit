using System;
using System.Collections.Generic;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.App.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.App
{
    public class MugenApplicationTest : UnitTestBase
    {
        private readonly MugenApplication _application;

        public MugenApplicationTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _application = new MugenApplication(null, ComponentCollectionManager);
        }

        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            _application.Metadata.ShouldNotBeNull();
            _application.HasMetadata.ShouldBeFalse();
            _application.Components.ShouldNotBeNull();
            _application.HasComponents.ShouldBeFalse();
            var deviceInfo = _application.PlatformInfo;
            deviceInfo.ShouldNotBeNull();
            deviceInfo.Idiom.ShouldEqual(PlatformIdiom.Unknown);
            deviceInfo.Type.ShouldEqual(new PlatformType("-"));
            deviceInfo.ApplicationVersion.ShouldEqual("0.0");
            deviceInfo.DeviceVersion.ShouldEqual("0.0");
            deviceInfo.Metadata.ShouldNotBeNull();
            MugenService.Application.ShouldEqual(_application);
        }

        [Fact]
        public void InitializeShouldBeHandledByComponent()
        {
            var state = this;
            var states = new List<ApplicationLifecycleState>();
            var device = new PlatformInfo(PlatformType.UnitTest, new MetadataContext());
            using var app = _application.AddComponent(new TestApplicationLifecycleListener(_application)
            {
                OnLifecycleChanged = (viewModelLifecycleState, st, metadata) =>
                {
                    states.Add(viewModelLifecycleState);
                    st.ShouldEqual(state);
                    metadata.ShouldEqual(DefaultMetadata);
                }
            });
            _application.Initialize(device, state, DefaultMetadata);
            _application.PlatformInfo.ShouldEqual(device);
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
            var target = _application;
            var state = ApplicationLifecycleState.Activated;

            _application.IsInState(state, DefaultMetadata).ShouldBeFalse();

            for (var i = 0; i < componentCount; i++)
            {
                var isLast = i - 1 == componentCount;
                var component = new TestLifecycleTrackerComponent<ApplicationLifecycleState>(_application)
                {
                    IsInState = (o, t, s, m) =>
                    {
                        ++count;
                        t.ShouldEqual(target);
                        m.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return true;
                        return false;
                    },
                    Priority = -i
                };
                _application.Components.TryAdd(component);
            }

            _application.IsInState(state, DefaultMetadata).ShouldBeFalse();
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
                var component = new TestApplicationLifecycleListener(_application)
                {
                    OnLifecycleChanged = (viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                _application.AddComponent(component);
            }

            _application.OnLifecycleChanged(lifecycleState, state, DefaultMetadata);
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
                var component = new TestUnhandledExceptionHandlerComponent(_application)
                {
                    OnUnhandledException = (e, t, metadata) =>
                    {
                        ++invokeCount;
                        e.ShouldEqual(ex);
                        t.ShouldEqual(type);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                _application.AddComponent(component);
            }

            _application.OnUnhandledException(ex, type, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }
    }
}