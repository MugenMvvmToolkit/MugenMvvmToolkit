using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldSetDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            using var t = MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(accessorMemberInfo)
            });

            var viewModel = new TestInitializableViewModel {Service = new Messenger()};
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer {SetDataContext = true});
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, this, DefaultMetadata);

            view.Target.BindableMembers().DataContext().ShouldEqual(viewModel);
        }

        [Fact]
        public void ShouldSubscribeViewModel()
        {
            var invokeCount = 0;
            var viewModel = new TestInitializableViewModel {Service = new Messenger()};
            viewModel.Service.AddComponent(new TestMessengerSubscriberComponent
            {
                TrySubscribe = (o, m, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(this);
                    m.ShouldEqual(ThreadExecutionMode.Main);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            });
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer {SetDataContext = false});
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, this, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldInitializeInitializableViews()
        {
            var state = "t";
            var invokeCount = 0;
            var componentInvokeCount = 0;
            IView? view = null;
            var rawView = new TestInitializableView
            {
                Initialize = (v, o, arg3) =>
                {
                    invokeCount++;
                    v.ShouldEqual(view);
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var componentView = new TestInitializableView
            {
                Initialize = (v, o, arg3) =>
                {
                    componentInvokeCount++;
                    v.ShouldEqual(view);
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };

            var viewModel = new TestInitializableViewModel();
            view = new View(new ViewMapping("1", typeof(IViewModelBase), rawView.GetType()), rawView, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer {SetDataContext = false});
            view.Components.TryAdd(componentView);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);

            state = null;
            view.Components.Remove(componentView);
            view.Components.TryAdd(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, state, DefaultMetadata);
            view.Components.Remove(componentView);
            view.Components.TryAdd(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);
        }

        #endregion

        #region Nested types

        private sealed class TestInitializableViewModel : TestViewModel, IHasService<IMessenger>
        {
            #region Properties

            public IMessenger Service { get; set; } = null!;

            public IMessenger ServiceOptional => throw new NotSupportedException();

            #endregion
        }

        private sealed class TestInitializableView : IInitializableView
        {
            #region Properties

            public Action<IView, object?, IReadOnlyMetadataContext?>? Initialize { get; set; }

            #endregion

            #region Implementation of interfaces

            void IInitializableView.Initialize(IView view, object? state, IReadOnlyMetadataContext? metadata) => Initialize?.Invoke(view, state, metadata);

            #endregion
        }

        #endregion
    }
}