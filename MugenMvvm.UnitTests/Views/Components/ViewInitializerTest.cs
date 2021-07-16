using System;
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
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Messaging;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    [Collection(SharedContext)]
    public class ViewInitializerTest : UnitTestBase
    {
        private readonly View _view;
        private readonly TestInitializableView _rawView;
        private readonly TestInitializableViewModel _viewModel;
        private readonly ViewInitializer _viewInitializer;

        public ViewInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestInitializableViewModel();
            _rawView = new TestInitializableView();
            _view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(TestInitializableView), Metadata), _rawView, _viewModel, null, ComponentCollectionManager);
            _viewInitializer = new ViewInitializer { SetDataContext = false };
            ViewManager.AddComponent(_viewInitializer);
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(AttachedValueManager));
        }

        [Fact]
        public void ShouldInitializeInitializableViews()
        {
            var state = "t";
            var invokeCount = 0;
            var componentInvokeCount = 0;

            _rawView.Initialize = (v, o, arg3) =>
            {
                invokeCount++;
                v.ShouldEqual(_view);
                o.ShouldEqual(state);
                arg3.ShouldEqual(Metadata);
            };
            var componentView = new TestInitializableView
            {
                Initialize = (v, o, arg3) =>
                {
                    componentInvokeCount++;
                    v.ShouldEqual(_view);
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(Metadata);
                }
            };

            _view.Components.TryAdd(componentView);
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing, state, Metadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);

            state = null;
            _view.Components.Remove(componentView);
            _view.Components.TryAdd(componentView, Metadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Clearing, state, Metadata);
            _view.Components.Remove(componentView);
            _view.Components.TryAdd(componentView, Metadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(2);
        }

        [Fact]
        public void ShouldSetDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, type, memberType, arg3, arg4, arg5) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(accessorMemberInfo)
            });

            _viewInitializer.SetDataContext = true;
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing, this, Metadata);
            _view.Target.BindableMembers().DataContext().ShouldEqual(_viewModel);
        }

        [Fact]
        public void ShouldSubscribeViewModel()
        {
            var invokeCount = 0;
            _viewModel.Service = new Messenger(ComponentCollectionManager);
            _viewModel.Service.AddComponent(new TestMessengerSubscriberComponent
            {
                TrySubscribe = (_, o, m, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(_view.Target);
                    m.ShouldEqual(ThreadExecutionMode.Main);
                    arg3.ShouldEqual(Metadata);
                    return true;
                }
            });

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing, this, Metadata);
            invokeCount.ShouldEqual(1);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        private sealed class TestInitializableViewModel : TestViewModel, IHasService<IMessenger>
        {
            public IMessenger Service { get; set; } = null!;

            IMessenger IHasService<IMessenger>.GetService(bool optional)
            {
                optional.ShouldBeFalse();
                return Service;
            }
        }

        private sealed class TestInitializableView : IInitializableView
        {
            public Action<IView, object?, IReadOnlyMetadataContext?>? Initialize { get; set; }

            void IInitializableView.Initialize(IView view, object? state, IReadOnlyMetadataContext? metadata) => Initialize?.Invoke(view, state, metadata);
        }
    }
}