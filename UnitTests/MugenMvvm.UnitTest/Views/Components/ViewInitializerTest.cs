using System;
using System.Collections.Generic;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Messaging.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldSetDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            TestComponentSubscriber.Subscribe(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg5, arg6) => ItemOrList.FromRawValue<IMemberInfo, IReadOnlyList<IMemberInfo>>(accessorMemberInfo)
            });

            var viewModel = new TestInitializableViewModel { Service = new Messenger() };
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer { SetDataContext = true });
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, this, DefaultMetadata);

            view.Target.BindableMembers().DataContext().ShouldEqual(viewModel);
        }

        [Fact]
        public void ShouldSubscribeViewModel()
        {
            var invokeCount = 0;
            var viewModel = new TestInitializableViewModel { Service = new Messenger() };
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
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), this, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer { SetDataContext = false });
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, this, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldInitializeInitializableViews()
        {
            string? state = "t";
            var invokeCount = 0;
            var componentInvokeCount = 0;
            IView? view = null;
            var rawView = new TestInitializableView
            {
                Initialize = (v, o, type, arg3) =>
                {
                    invokeCount++;
                    v.ShouldEqual(view);
                    o.ShouldEqual(state);
                    if (state != null)
                        type.ShouldEqual(state.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };
            var componentView = new TestInitializableView
            {
                Initialize = (v, o, type, arg3) =>
                {
                    componentInvokeCount++;
                    v.ShouldEqual(view);
                    o.ShouldEqual(state);
                    if (state != null)
                        type.ShouldEqual(state.GetType());
                    arg3.ShouldEqual(DefaultMetadata);
                }
            };

            var viewModel = new TestInitializableViewModel();
            view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), rawView, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewInitializer { SetDataContext = false });
            view.Components.Add(componentView);
            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initializing, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);

            state = null;
            view.Components.Remove(componentView);
            view.Components.Add(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, state, DefaultMetadata);
            view.Components.Remove(componentView);
            view.Components.Add(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);
        }

        #endregion

        #region Nested types

        private sealed class TestInitializableViewModel : TestViewModel, IHasOptionalService<IMessenger>
        {
            #region Properties

            public IMessenger? Service { get; set; }

            #endregion
        }

        private sealed class TestInitializableView : IInitializableView
        {
            #region Properties

            public Action<IView, object?, Type, IReadOnlyMetadataContext?>? Initialize { get; set; }

            #endregion

            #region Implementation of interfaces

            void IInitializableView.Initialize<TState>(IView view, in TState state, IReadOnlyMetadataContext? metadata)
            {
                Initialize?.Invoke(view, state, typeof(TState), metadata);
            }

            #endregion
        }

        #endregion
    }
}