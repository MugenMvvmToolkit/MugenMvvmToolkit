using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Tests.ViewModels;
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
        public ViewModelViewAwareInitializerTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ViewManager.AddComponent(new ViewModelViewAwareInitializer(WrapperManager, ReflectionManager));
        }

        [Fact]
        public void ShouldSetView1()
        {
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), typeof(AwareViewBase)), new AwareViewBase(), viewModel, null, ComponentCollectionManager);

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, Metadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldBeNull();

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, Metadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();

            var wrapper = new AwareView();
            WrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            WrapperManager.AddComponent(new DelegateWrapperManager<AwareViewBase, AwareViewBase>((type, v, arg4) =>
            {
                arg4.ShouldEqual(Metadata);
                v.ShouldEqual(view.Target);
                return type == typeof(AwareView);
            }, (type, v, arg4) =>
            {
                arg4.ShouldEqual(Metadata);
                v.ShouldEqual(view.Target);
                type.ShouldEqual(typeof(AwareView));
                return wrapper;
            }));

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, Metadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(wrapper);
        }

        [Fact]
        public void ShouldSetView2()
        {
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), typeof(AwareView)), new AwareView(), viewModel, null, ComponentCollectionManager);

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, Metadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(view.Target);

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, Metadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();
        }

        [Fact]
        public void ShouldSetViewModel1()
        {
            var viewModel = new AwareViewModelBase();
            var rawView = new AwareView();
            var view = new View(new ViewMapping("1", typeof(IViewModelBase), rawView.GetType()), rawView, viewModel, null, ComponentCollectionManager);

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, Metadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldBeNull();

            var rawViewComponent = new AwareView();
            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, Metadata);
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

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, Metadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldEqual(viewModel);

            var rawViewComponent = new AwareView();
            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldEqual(viewModel);

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            ViewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, Metadata);
            rawView.ViewModelBase.ShouldBeNull();
            rawView.ViewModel.ShouldBeNull();
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.TryAdd(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();
        }

        protected override IWrapperManager GetWrapperManager() => new WrapperManager(ComponentCollectionManager);

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

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