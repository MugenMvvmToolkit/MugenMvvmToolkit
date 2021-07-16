using System.Threading.Tasks;
using MugenMvvm.Busy.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Messaging;
using MugenMvvm.Tests.Presentation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using MugenMvvm.Views;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class ViewModelCleanerTest : UnitTestBase
    {
        public ViewModelCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ViewModelManager.AddComponent(new ViewModelCleaner(Presenter, ViewManager, AttachedValueManager));
        }

        [Fact]
        public void ShouldClearAttachedValues()
        {
            const string attachedPath = "t";
            var viewModel = new TestCleanerViewModel();
            AttachedValueManager.TryGetAttachedValues(viewModel).Set(attachedPath, this, out _);
            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            AttachedValueManager.TryGetAttachedValues(viewModel).Contains(attachedPath).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearBusyManager()
        {
            BusyManager.ClearComponents();
            var viewModel = new TestCleanerViewModel
            {
                BusyManager = BusyManager
            };
            viewModel.BusyManager.AddComponent(new BusyTokenManager());
            var busyToken = viewModel.BusyManager.BeginBusy(this);
            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            busyToken.IsCompleted.ShouldBeTrue();
            viewModel.BusyManager.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMessenger()
        {
            Messenger.ClearComponents();
            var viewModel = new TestCleanerViewModel
            {
                Messenger = Messenger
            };
            viewModel.Messenger.AddComponent(new MessengerHandlerSubscriber(ReflectionManager));
            viewModel.Messenger.TrySubscribe(new TestMessengerHandlerRaw()).ShouldBeTrue();
            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Messenger.GetSubscribers().ShouldBeEmpty();
            viewModel.Messenger.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            var viewModel = new TestCleanerViewModel();
            viewModel.Metadata.Set(ViewModelMetadata.ViewModel, viewModel);
            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Metadata.TryGet(ViewModelMetadata.ViewModel, out var vm).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearViews()
        {
            var viewModel = new TestCleanerViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, viewModel);

            ViewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (_, o, arg3) =>
                {
                    o.ShouldEqual(viewModel);
                    arg3.ShouldEqual(Metadata);
                    return view;
                }
            });

            var cleanupCount = 0;
            ViewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (_, v, o, arg4, arg5) =>
                {
                    ++cleanupCount;
                    v.ShouldEqual(view);
                    arg4.ShouldEqual(Metadata);
                    return new ValueTask<bool>(true);
                }
            });

            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this, Metadata);
            cleanupCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldClearWeakReference()
        {
            var viewModel = new TestCleanerViewModel();
            viewModel.Value = new WeakReferenceImpl(viewModel, false);
            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Value.Target.ShouldBeNull();
        }

        [Fact]
        public void ShouldCloseViews()
        {
            var closeCount = 0;
            var viewModel = new TestCleanerViewModel();
            Presenter.AddComponent(new TestPresenterComponent
            {
                TryClose = (_, o, m, _) =>
                {
                    o.ShouldEqual(viewModel);
                    m!.Get(NavigationMetadata.ForceClose).ShouldBeTrue();
                    ++closeCount;
                    return default;
                }
            });

            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposing, this, Metadata);
            closeCount.ShouldEqual(1);

            ViewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this, Metadata);
            closeCount.ShouldEqual(1);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        private sealed class TestCleanerViewModel : TestViewModel, IHasService<IBusyManager>, IHasService<IMessenger>, IValueHolder<IWeakReference>
        {
            public IBusyManager? BusyManager { get; set; }

            public IMessenger? Messenger { get; set; }

            public IWeakReference? Value { get; set; }

            IBusyManager? IHasService<IBusyManager>.GetService(bool optional)
            {
                optional.ShouldBeTrue();
                return BusyManager;
            }

            IMessenger? IHasService<IMessenger>.GetService(bool optional)
            {
                optional.ShouldBeTrue();
                return Messenger;
            }
        }
    }
}