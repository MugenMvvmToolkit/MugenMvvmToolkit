using System;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Messaging.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewCleanerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldUnsubscribeViewModel()
        {
            var invokeCount = 0;
            var viewModel = new TestCleanableViewModel { Service = new Messenger() };
            viewModel.Service.AddComponent(new TestMessengerSubscriberComponent
            {
                TryUnsubscribe = (o, type, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            });
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner());
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldCleanCleanableViews()
        {
            string? state = "t";
            int invokeCount = 0;
            int componentInvokeCount = 0;
            var rawView = new TestCleanableView
            {
                Cleanup = (o, type, arg3) =>
                {
                    invokeCount++;
                    o.ShouldEqual(state);
                    if (state != null)
                        type.ShouldEqual(state.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var componentView = new TestCleanableView
            {
                Cleanup = (o, type, arg3) =>
                {
                    componentInvokeCount++;
                    o.ShouldEqual(state);
                    if (state != null)
                        type.ShouldEqual(state.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var viewModel = new TestCleanableViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), rawView, viewModel);
            view.Components.Add(componentView);
            view.Components.Components.Add(this);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner());

            state = null;
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, state, DefaultMetadata);
            view.Components.Remove(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(0);
            componentInvokeCount.ShouldEqual(1);
            view.Components.Add(componentView, DefaultMetadata);
            componentInvokeCount = 0;
            state = "t";

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);
            view.Components.Count.ShouldEqual(0);
            view.Components.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            var viewModel = new TestCleanableViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner());
            view.Metadata.Set(ViewMetadata.LifecycleState, ViewLifecycleState.Clearing);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            view.Metadata.Count.ShouldEqual(0);
        }

        #endregion

        #region Nested types

        private sealed class TestCleanableViewModel : TestViewModel, IHasOptionalService<IMessenger>
        {
            #region Properties

            public IMessenger? Service { get; set; }

            #endregion
        }

        private sealed class TestCleanableView : ICleanableView
        {
            #region Properties

            public Action<object?, Type, IReadOnlyMetadataContext?>? Cleanup { get; set; }

            #endregion

            #region Implementation of interfaces

            void ICleanableView.Cleanup<TState>(in TState state, IReadOnlyMetadataContext? metadata)
            {
                Cleanup?.Invoke(state, typeof(TState), metadata);
            }

            #endregion
        }

        #endregion
    }
}