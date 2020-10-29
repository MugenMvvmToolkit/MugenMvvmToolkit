using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewCleanerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldClearDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            using var t = MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg6) => ItemOrList.FromRawValue<IMemberInfo, IReadOnlyList<IMemberInfo>>(accessorMemberInfo)
            });


            var viewModel = new TestCleanableViewModel {ServiceOptional = new Messenger()};
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner {ClearDataContext = true});
            view.Target.BindableMembers().SetDataContext(viewModel);
            view.Target.BindableMembers().DataContext().ShouldEqual(viewModel);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            view.Target.BindableMembers().DataContext().ShouldBeNull();
        }

        [Fact]
        public void ShouldUnsubscribeViewModel()
        {
            var invokeCount = 0;
            var viewModel = new TestCleanableViewModel {ServiceOptional = new Messenger()};
            viewModel.ServiceOptional.AddComponent(new TestMessengerSubscriberComponent
            {
                TryUnsubscribe = (o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            });
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner());
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldCleanCleanableViews()
        {
            var state = "t";
            var invokeCount = 0;
            var componentInvokeCount = 0;
            var rawView = new TestCleanableView
            {
                Cleanup = (o, arg3) =>
                {
                    invokeCount++;
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var componentView = new TestCleanableView
            {
                Cleanup = (o, arg3) =>
                {
                    componentInvokeCount++;
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var viewModel = new TestCleanableViewModel();
            var view = new View(new ViewMapping("1", rawView.GetType(), typeof(IViewModelBase)), rawView, viewModel);
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
            var view = new View(new ViewMapping("1", GetType(), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewCleaner());
            view.Metadata.Set(ViewModelMetadata.Id, "");
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            view.Metadata.Count.ShouldEqual(0);
        }

        #endregion

        #region Nested types

        private sealed class TestCleanableViewModel : TestViewModel, IHasService<IMessenger>
        {
            #region Properties

            public IMessenger Service => throw new NotSupportedException();

            public IMessenger? ServiceOptional { get; set; }

            #endregion
        }

        private sealed class TestCleanableView : ICleanableView
        {
            #region Properties

            public Action<object?, IReadOnlyMetadataContext?>? Cleanup { get; set; }

            #endregion

            #region Implementation of interfaces

            void ICleanableView.Cleanup(object? state, IReadOnlyMetadataContext? metadata) => Cleanup?.Invoke(state, metadata);

            #endregion
        }

        #endregion
    }
}