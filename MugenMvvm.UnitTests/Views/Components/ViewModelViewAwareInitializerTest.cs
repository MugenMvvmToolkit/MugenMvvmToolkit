using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewModelViewAwareInitializerTest : UnitTestBase
    {
        private readonly WrapperManager _wrapperManager;
        private readonly ViewManager _viewManager;

        public ViewModelViewAwareInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _wrapperManager = new WrapperManager(ComponentCollectionManager);
            _viewManager = new ViewManager(ComponentCollectionManager);
            _viewManager.AddComponent(new ViewModelViewAwareInitializer(_wrapperManager));
        }

        [Fact]
        public void ShouldSetView1()
        {
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), typeof(AwareViewBase)), new AwareViewBase(), viewModel, null, ComponentCollectionManager);

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldBeNull();

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();

            var wrapper = new AwareView();
            _wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            _wrapperManager.AddComponent(new DelegateWrapperManager<AwareViewBase, AwareViewBase>((type, v, arg4) =>
            {
                arg4.ShouldEqual(DefaultMetadata);
                v.ShouldEqual(view.Target);
                return type == typeof(AwareView);
            }, (type, v, arg4) =>
            {
                arg4.ShouldEqual(DefaultMetadata);
                v.ShouldEqual(view.Target);
                type.ShouldEqual(typeof(AwareView));
                return wrapper;
            }));

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(wrapper);
        }

        [Fact]
        public void ShouldSetView2()
        {
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), typeof(AwareView)), new AwareView(), viewModel, null, ComponentCollectionManager);

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(view.Target);

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();
        }

        [Fact]
        public void ShouldSetViewModel1()
        {
            var viewModel = new AwareViewModelBase();
            var rawView = new AwareView();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), rawView.GetType()), rawView, viewModel, null, ComponentCollectionManager);

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldBeNull();

            var rawViewComponent = new AwareView();
            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldBeNull();
            rawView.ViewModel.ShouldBeNull();
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();
        }

        [Fact]
        public void ShouldSetViewModel2()
        {
            var viewModel = new AwareViewModel();
            var rawView = new AwareView();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), rawView.GetType()), rawView, viewModel, null, ComponentCollectionManager);

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldEqual(viewModel);

            var rawViewComponent = new AwareView();
            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldEqual(viewModel);

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            _viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldBeNull();
            rawView.ViewModel.ShouldBeNull();
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();
        }

        private class AwareViewBase
        {
        }

        private class AwareView : AwareViewBase, IViewModelAwareView<AwareViewModelBase>, IViewModelAwareView<AwareViewModel>
        {
            public AwareViewModel? ViewModel;

            public AwareViewModelBase? ViewModelBase;

            AwareViewModel? IViewModelAwareView<AwareViewModel>.ViewModel
            {
                get => ViewModel;
                set => ViewModel = value;
            }

            AwareViewModelBase? IViewModelAwareView<AwareViewModelBase>.ViewModel
            {
                get => ViewModelBase;
                set => ViewModelBase = value;
            }
        }

        private class AwareViewModelBase : TestViewModel
        {
        }

        private class AwareViewModel : AwareViewModelBase, IViewAwareViewModel<AwareViewBase>, IViewAwareViewModel<AwareView>
        {
            public AwareView? View;
            public AwareViewBase? ViewBase;

            AwareView? IViewAwareViewModel<AwareView>.View
            {
                get => View;
                set => View = value;
            }

            AwareViewBase? IViewAwareViewModel<AwareViewBase>.View
            {
                get => ViewBase;
                set => ViewBase = value;
            }
        }
    }
}