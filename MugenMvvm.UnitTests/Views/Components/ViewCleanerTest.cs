using System;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    [Collection(SharedContext)]
    public class ViewCleanerTest : UnitTestBase, IDisposable
    {
        private readonly View _view;
        private readonly TestCleanableView _rawView;
        private readonly TestCleanableViewModel _viewModel;
        private readonly ViewManager _viewManager;
        private readonly ViewCleaner _viewCleaner;

        public ViewCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewModel = new TestCleanableViewModel();
            _rawView = new TestCleanableView();
            _view = new View(new ViewMapping("id", typeof(TestViewModel), typeof(TestCleanableView), DefaultMetadata), _rawView, _viewModel, null, ComponentCollectionManager);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewCleaner = new ViewCleaner(AttachedValueManager);
            _viewManager.AddComponent(_viewCleaner);
            MugenService.Configuration.InitializeInstance<IMemberManager>(new MemberManager(ComponentCollectionManager));
            MugenService.Configuration.InitializeInstance<IAttachedValueManager>(AttachedValueManager);
        }

        public void Dispose()
        {
            MugenService.Configuration.Clear<IMemberManager>();
            MugenService.Configuration.Clear<IAttachedValueManager>();
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
                arg3.ShouldEqual(DefaultMetadata);
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

            _view.Components.TryAdd(componentView);
            _view.Components.Components.TryAdd(this);

            state = null;
            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Initializing, state, DefaultMetadata);
            _view.Components.Remove(componentView, DefaultMetadata);
            invokeCount.ShouldEqual(0);
            componentInvokeCount.ShouldEqual(1);
            _view.Components.TryAdd(componentView, DefaultMetadata);
            componentInvokeCount = 0;
            state = "t";

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, state, DefaultMetadata);
            invokeCount.ShouldEqual(1);
            componentInvokeCount.ShouldEqual(1);
            _view.Components.Count.ShouldEqual(0);
            _view.Components.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearDataContext()
        {
            var accessorMemberInfo = BindableMembers.For<object>().DataContext().GetBuilder().Build();
            MugenService.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (type, memberType, arg3, arg4, arg6) => ItemOrIReadOnlyList.FromRawValue<IMemberInfo>(accessorMemberInfo)
            });

            _viewCleaner.ClearDataContext = true;
            _viewModel.ServiceOptional = new Messenger(ComponentCollectionManager);
            _view.Target.BindableMembers().SetDataContext(_viewModel);
            _view.Target.BindableMembers().DataContext().ShouldEqual(_viewModel);
            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            _view.Target.BindableMembers().DataContext().ShouldBeNull();
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            _view.Metadata.Set(ViewModelMetadata.Id, "");
            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            _view.Metadata.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldUnsubscribeViewModel()
        {
            var invokeCount = 0;
            _viewModel.ServiceOptional = new Messenger(ComponentCollectionManager);
            _viewModel.ServiceOptional.AddComponent(new TestMessengerSubscriberComponent
            {
                TryUnsubscribe = (o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(_view.Target);
                    arg3.ShouldEqual(DefaultMetadata);
                    return true;
                }
            });

            _viewManager.OnLifecycleChanged(_view, ViewLifecycleState.Cleared, this, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

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