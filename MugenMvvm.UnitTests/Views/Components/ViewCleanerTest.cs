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
using MugenMvvm.Metadata;
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
    public class ViewCleanerTest : UnitTestBase
    {
        private readonly View _view;
        private readonly TestCleanableView _rawView;
        private readonly TestCleanableViewModel _viewModel;
        private readonly ViewCleaner _viewCleaner;

        public ViewCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestCleanableViewModel();
            _rawView = new TestCleanableView();
            _view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(TestCleanableView), Metadata), _rawView, _viewModel, null, ComponentCollectionManager);
            _viewCleaner = new ViewCleaner(AttachedValueManager);
            ViewManager.AddComponent(_viewCleaner);
            RegisterDisposeToken(WithGlobalService(MemberManager));
            RegisterDisposeToken(WithGlobalService(AttachedValueManager));
        }

        [Fact]
        public void ShouldCleanCleanableViews()
        {
            var state = "t";
            var invokeCount = 0;
            var componentInvokeCount = 0;

            _rawView.Cleanup = (o, arg3) =>
            {
                invokeCount++;
                o.ShouldEqual(state);
                arg3.ShouldEqual(Metadata);
            };
            var componentView = new TestCleanableView
            {
                Cleanup = (o, arg3) =>
                {
                    componentInvokeCount++;
                    o.ShouldEqual(state);
                    arg3.ShouldEqual(Metadata);
                }
            };

            _view.Components.TryAdd(componentView);
            _view.Components.Components.TryAdd(this);

            state = null;
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing, state, Metadata);
            _view.Components.Remove(componentView, Metadata);
            invokeCount.ShouldEqual(0);
            componentInvokeCount.ShouldEqual(1);
            _view.Components.TryAdd(componentView, Metadata);
            componentInvokeCount = 0;
            state = "t";

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, state, Metadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);
            _view.Components.Count.ShouldEqual(0);
            _view.Components.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (_, type, memberType, arg3, arg4, arg6) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(accessorMemberInfo)
            });

            _viewCleaner.ClearDataContext = true;
            _viewModel.ServiceOptional = new Messenger(ComponentCollectionManager);
            _view.Target.BindableMembers().SetDataContext(_viewModel);
            _view.Target.BindableMembers().DataContext().ShouldEqual(_viewModel);
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, Metadata);
            _view.Target.BindableMembers().DataContext().ShouldBeNull();
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            _view.Metadata.Set(ViewModelMetadata.Id, "");
            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, Metadata);
            _view.Metadata.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldUnsubscribeViewModel()
        {
            var invokeCount = 0;
            _viewModel.ServiceOptional = new Messenger(ComponentCollectionManager);
            _viewModel.ServiceOptional.AddComponent(new TestMessengerSubscriberComponent
            {
                TryUnsubscribe = (_, o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(_view.Target);
                    arg3.ShouldEqual(Metadata);
                    return true;
                }
            });

            ViewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, Metadata);
            invokeCount.ShouldEqual(1);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        private sealed class TestCleanableViewModel : TestViewModel, IHasService<IMessenger>
        {
            public IMessenger? ServiceOptional { get; set; }

            IMessenger? IHasService<IMessenger>.GetService(bool optional)
            {
                optional.ShouldBeTrue();
                return ServiceOptional;
            }
        }

        private sealed class TestCleanableView : ICleanableView
        {
            public Action<object?, IReadOnlyMetadataContext?>? Cleanup { get; set; }

            void ICleanableView.Cleanup(object? state, IReadOnlyMetadataContext? metadata) => Cleanup?.Invoke(state, metadata);
        }
    }
}