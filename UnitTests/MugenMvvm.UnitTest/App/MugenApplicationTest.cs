using System.Collections.Generic;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.App.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.App
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
            var deviceInfo = mugenApplication.DeviceInfo;
            deviceInfo.ShouldNotBeNull();
            deviceInfo.Idiom.ShouldEqual(PlatformIdiom.Unknown);
            deviceInfo.Platform.ShouldEqual(PlatformType.Unknown);
            deviceInfo.RawVersion.ShouldEqual("0");
            deviceInfo.Metadata.ShouldNotBeNull();
            MugenService.Application.ShouldEqual(mugenApplication);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void OnLifecycleChangedShouldBeHandledByComponents(int count)
        {
            var manager = new MugenApplication();
            var invokeCount = 0;
            var state = "state";
            var lifecycleState = ApplicationLifecycleState.Initialized;
            for (var i = 0; i < count; i++)
            {
                var component = new TestApplicationLifecycleDispatcherComponent
                {
                    OnLifecycleChanged = (app, viewModelLifecycleState, st, metadata) =>
                    {
                        ++invokeCount;
                        app.ShouldEqual(manager);
                        st.ShouldEqual(state);
                        viewModelLifecycleState.ShouldEqual(lifecycleState);
                        metadata.ShouldEqual(DefaultMetadata);
                    },
                    Priority = i
                };
                manager.AddComponent(component);
            }

            manager.OnLifecycleChanged(lifecycleState, state, DefaultMetadata);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void InitializeShouldBeHandledByComponent()
        {
            var state = this;
            var states = new List<ApplicationLifecycleState>();
            var device = new DeviceInfo(PlatformType.UnitTest, PlatformIdiom.Phone, "00", new MetadataContext());
            var application = new MugenApplication();
            application.AddComponent(new TestApplicationLifecycleDispatcherComponent
            {
                OnLifecycleChanged = (app, viewModelLifecycleState, st, metadata) =>
                {
                    states.Add(viewModelLifecycleState);
                    app.ShouldEqual(application);
                    st.ShouldEqual(state);
                    metadata.ShouldEqual(DefaultMetadata);
                }
            });
            application.Initialize(device, state, DefaultMetadata);
            application.DeviceInfo.ShouldEqual(device);
            states.Count.ShouldEqual(2);
            states[0].ShouldEqual(ApplicationLifecycleState.Initializing);
            states[1].ShouldEqual(ApplicationLifecycleState.Initialized);
        }

        #endregion
    }
}