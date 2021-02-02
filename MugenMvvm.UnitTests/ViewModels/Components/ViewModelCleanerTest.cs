using System.Threading.Tasks;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Messaging.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
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
        private readonly ViewModelManager _viewModelManager;
        private readonly ViewManager _viewManager;

        public ViewModelCleanerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewModelManager = new ViewModelManager(ComponentCollectionManager);
            _viewModelManager.AddComponent(new ViewModelCleaner(_viewManager, AttachedValueManager));
        }

        [Fact]
        public void ShouldClearAttachedValues()
        {
            const string attachedPath = "t";
            var viewModel = new TestCleanerViewModel();
            AttachedValueManager.TryGetAttachedValues(viewModel).Set(attachedPath, this, out _);
            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            AttachedValueManager.TryGetAttachedValues(viewModel).Contains(attachedPath).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearBusyManager()
        {
            var viewModel = new TestCleanerViewModel
            {
                BusyManager = new BusyManager(ComponentCollectionManager)
            };
            viewModel.BusyManager.AddComponent(new BusyTokenManager());
            var busyToken = viewModel.BusyManager.BeginBusy(this);
            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            busyToken.IsCompleted.ShouldBeTrue();
            viewModel.BusyManager.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMessenger()
        {
            var viewModel = new TestCleanerViewModel
            {
                Messenger = new Messenger(ComponentCollectionManager)
            };
            viewModel.Messenger.AddComponent(new MessengerHandlerSubscriber(ReflectionManager));
            viewModel.Messenger.TrySubscribe(new TestMessengerHandlerRaw()).ShouldBeTrue();
            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Messenger.GetSubscribers().AsList().ShouldBeEmpty();
            viewModel.Messenger.Components.Count.ShouldEqual(0);
        }

        [Fact]
        public void ShouldClearMetadata()
        {
            var viewModel = new TestCleanerViewModel();
            viewModel.Metadata.Set(ViewModelMetadata.ViewModel, viewModel);
            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Metadata.TryGet(ViewModelMetadata.ViewModel, out var vm).ShouldBeFalse();
        }

        [Fact]
        public void ShouldClearViews()
        {
            var viewModel = new TestCleanerViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), GetType()), this, viewModel);

            _viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, arg3) =>
                {
                    o.ShouldEqual(viewModel);
                    arg3.ShouldEqual(DefaultMetadata);
                    return view;
                }
            });

            var cleanupCount = 0;
            _viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, o, arg4, arg5) =>
                {
                    ++cleanupCount;
                    v.ShouldEqual(view);
                    arg4.ShouldEqual(DefaultMetadata);
                    return new ValueTask<bool>(true);
                }
            });

            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this, DefaultMetadata);
            cleanupCount.ShouldEqual(1);
        }

        [Fact]
        public void ShouldClearWeakReference()
        {
            var viewModel = new TestCleanerViewModel();
            viewModel.Value = new WeakReferenceImpl(viewModel, false);
            _viewModelManager.OnLifecycleChanged(viewModel, ViewModelLifecycleState.Disposed, this);
            viewModel.Value.Target.ShouldBeNull();
        }

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