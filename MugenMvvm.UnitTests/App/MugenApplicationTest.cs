using System;
using System.Collections.Generic;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.App.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.App
{
    public class MugenApplicationTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeDefaultValues()
        {
            var mugenApplication = new MugenApplication();
            mugenApplication.Metadata.ShouldNotBeNull();
            mugenApplication.HasMetadata.ShouldBeFalse();
            mugenApplication.Components.ShouldNotBeNull();
            mugenApplication.HasComponents.ShouldBeFalse();
            var deviceInfo = mugenApplication.PlatformInfo;
            deviceInfo.ShouldNotBeNull();
            deviceInfo.Idiom.ShouldEqual(PlatformIdiom.Unknown);
            deviceInfo.Type.ShouldEqual(PlatformType.Unknown);
            deviceInfo.ApplicationVersion.ShouldEqual("0.0");
            deviceInfo.DeviceVersion.ShouldEqual("0.0");
            deviceInfo.Metadata.ShouldNotBeNull();
            MugenService.Application.ShouldEqual(mugenApplication);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var application = new MugenApplication();
            var invokeCount = 0;
            var state = "state";
            var lifecycleState = ApplicationLifecycleState.Initialized;
            for (var i = 0; i < count; i++)
            {
                var component = new TestApplicationLifecycleDispatcherComponent(application)
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
                application.AddComponent(component);
            }

            application.OnLifecycleChanged(lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnUnhandledExceptionShouldBeHandledByComponents(int count)
        {
            var application = new MugenApplication();
            var invokeCount = 0;
            var type = UnhandledExceptionType.System;
            var ex = new Exception();
            for (var i = 0; i < count; i++)
            {
                var component = new TestApplicationUnhandledExceptionComponent(application)
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
                application.AddComponent(component);
            }

            application.OnUnhandledException(ex, type, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void InitializeShouldBeHandledByComponent()
        {
            var state = this;
            var states = new List<ApplicationLifecycleState>();
            var device = new PlatformInfo(PlatformType.UnitTest, new MetadataContext());
            var application = new MugenApplication();
            application.AddComponent(new TestApplicationLifecycleDispatcherComponent(application)
            {
                OnLifecycleChanged = (viewModelLifecycleState, st, metadata) =>
                {
                    states.Add(viewModelLifecycleState);
                    st.ShouldEqual(state);
                    metadata.ShouldEqual(DefaultMetadata);
                }
            });
            application.Initialize(device, state, DefaultMetadata);
            application.PlatformInfo.ShouldEqual(device);
            states.Count.ShouldEqual(2);
            states[0].ShouldEqual(ApplicationLifecycleState.Initializing);
            states[1].ShouldEqual(ApplicationLifecycleState.Initialized);
        }

        #endregion
    }
}