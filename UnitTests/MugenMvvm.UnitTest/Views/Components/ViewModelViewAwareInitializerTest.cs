using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewModelViewAwareInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ShouldSetView1()
        {
            var wrapperManager = new WrapperManager();
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), new AwareViewBase(), viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewModelViewAwareInitializer(wrapperManager));

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldBeNull();

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();

            var wrapper = new AwareView();
            wrapperManager.AddComponent(new ViewWrapperManagerDecorator());
            wrapperManager.AddComponent(new DelegateWrapperManager<AwareViewBase, AwareViewBase>((type, v, arg4) =>
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

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(wrapper);
        }

        [Fact]
        public void ShouldSetView2()
        {
            var viewModel = new AwareViewModel();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), new AwareView(), viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewModelViewAwareInitializer());

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            viewModel.ViewBase.ShouldEqual(view.Target);
            viewModel.View.ShouldEqual(view.Target);

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            viewModel.ViewBase.ShouldBeNull();
            viewModel.View.ShouldBeNull();
        }

        [Fact]
        public void ShouldSetViewModel1()
        {
            var wrapperManager = new WrapperManager();
            var viewModel = new AwareViewModelBase();
            var rawView = new AwareView();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), rawView, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewModelViewAwareInitializer(wrapperManager));

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldBeNull();

            var rawViewComponent = new AwareView();
            view.Components.Add(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldBeNull();
            rawView.ViewModel.ShouldBeNull();
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.Add(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();
        }

        [Fact]
        public void ShouldSetViewModel2()
        {
            var viewModel = new AwareViewModel();
            var rawView = new AwareView();
            var view = new View(new ViewMapping("1", typeof(string), typeof(IViewModelBase)), rawView, viewModel);
            var viewManager = new ViewManager();
            viewManager.AddComponent(new ViewModelViewAwareInitializer());

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Initialized, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldEqual(viewModel);
            rawView.ViewModel.ShouldEqual(viewModel);

            var rawViewComponent = new AwareView();
            view.Components.Add(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldEqual(viewModel);
            rawViewComponent.ViewModel.ShouldEqual(viewModel);

            view.Components.Remove(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            viewManager.OnLifecycleChanged(view, ViewLifecycleState.Clearing, this, DefaultMetadata);
            rawView.ViewModelBase.ShouldBeNull();
            rawView.ViewModel.ShouldBeNull();
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();

            view.Components.Add(rawViewComponent);
            rawViewComponent.ViewModelBase.ShouldBeNull();
            rawViewComponent.ViewModel.ShouldBeNull();
        }

        #endregion

        #region Nested types

        private class AwareViewBase
        {
        }

        private class AwareView : AwareViewBase, IViewModelAwareView<AwareViewModelBase>, IViewModelAwareView<AwareViewModel>
        {
            #region Fields

            public AwareViewModel? ViewModel;

            public AwareViewModelBase? ViewModelBase;

            #endregion

            #region Properties

            AwareViewModelBase? IViewModelAwareView<AwareViewModelBase>.ViewModel
            {
                get => ViewModelBase;
                set => ViewModelBase = value;
            }

            AwareViewModel? IViewModelAwareView<AwareViewModel>.ViewModel
            {
                get => ViewModel;
                set => ViewModel = value;
            }

            #endregion
        }

        private class AwareViewModelBase : TestViewModel
        {
        }

        private class AwareViewModel : AwareViewModelBase, IViewAwareViewModel<AwareViewBase>, IViewAwareViewModel<AwareView>
        {
            #region Fields

            public AwareView? View;
            public AwareViewBase? ViewBase;

            #endregion

            #region Properties

            AwareViewBase? IViewAwareViewModel<AwareViewBase>.View
            {
                get => ViewBase;
                set => ViewBase = value;
            }

            AwareView? IViewAwareViewModel<AwareView>.View
            {
                get => View;
                set => View = value;
            }

            #endregion
        }

        #endregion
    }
}