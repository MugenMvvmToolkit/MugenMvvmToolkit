using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Presentation.Components;
using MugenMvvm.Requests;
using MugenMvvm.Tests.Presentation;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Tests.Views;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Presentation.Components
{
    public class ViewModelPresenterTest : UnitTestBase
    {
        public ViewModelPresenterTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            Presenter.AddComponent(new ViewModelPresenter(ViewManager));
        }

        [Fact]
        public void TryCloseShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            Presenter.TryClose(vm, default, Metadata).IsEmpty.ShouldBeTrue();
            Presenter.TryClose(this, default, Metadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryCloseShouldUseRegisteredMediators()
        {
            var viewModel = new TestViewModel();
            var view = new object();
            var request = new ViewModelViewRequest(viewModel, view);
            var result = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Popup);
            var closeCount = 0;
            var mediator = new TestViewModelPresenterMediator
            {
                TryClose = (v, token, context) =>
                {
                    ++closeCount;
                    v.ShouldEqual(request.View);
                    token.ShouldEqual(DefaultCancellationToken);
                    context.ShouldEqual(Metadata);
                    return result;
                }
            };
            var mapping = new ViewMapping("t", typeof(object), typeof(object), Metadata);
            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (_, _, _) => mapping
            });

            Presenter.AddComponent(ViewModelPresenterMediatorProvider.Get(typeof(object), false, (p, vm, m, meta) => mediator));
            Presenter.TryShow(viewModel, DefaultCancellationToken, Metadata);
            Presenter.TryClose(request, DefaultCancellationToken, Metadata).Single().ShouldEqual(result);
            closeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryShowShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            Presenter.TryShow(vm, default, Metadata).IsEmpty.ShouldBeTrue();
            Presenter.TryShow(this, default, Metadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldUseRegisteredMediators(bool isRawRequest)
        {
            var viewModel = new TestViewModel();
            var request = new ViewModelViewRequest(viewModel, new object());
            var mapping = new ViewMapping("t", typeof(TestViewModel), typeof(TestView1), Metadata);
            ViewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (_, o, m) =>
                {
                    if (isRawRequest)
                        o.ShouldEqual(viewModel);
                    else
                        o.ShouldEqual(request);

                    m.ShouldEqual(Metadata);
                    return mapping;
                }
            });

            var showCount = 0;
            var mediators = new List<TestViewModelPresenterMediator>();

            TestViewModelPresenterMediator Initialize(TestViewModelPresenterMediator instance)
            {
                instance.TryShow = (o, token, m) =>
                {
                    ++showCount;
                    o.ShouldEqual(isRawRequest ? null : request!.View);
                    token.ShouldEqual(DefaultCancellationToken);
                    m.ShouldEqual(Metadata);
                    return new PresenterResult(viewModel!, mapping!.Id, instance, NavigationType.Popup);
                };
                mediators!.Add(instance);
                return instance;
            }

            var wrapperManager = new WrapperManager(ComponentCollectionManager);
            var viewModelPresenter = new ViewModelPresenter(ViewManager);
            Presenter.AddComponent(ViewModelPresenterMediatorProvider.Get((p, vm, m, meta) =>
            {
                meta.ShouldEqual(Metadata);
                if (m.ViewType == typeof(TestView2))
                    return Initialize(new TestViewModelPresenterMediator<TestView2>());
                return null;
            }));
            Presenter.AddComponent(ViewModelPresenterMediatorProvider.Get(typeof(ViewModelPresenterTest), false,
                (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestView2>()), 1, wrapperManager));
            var c1 = ViewModelPresenterMediatorProvider.Get(typeof(TestViewBase), false, (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestViewBase>()), 2,
                wrapperManager);
            var c2 = ViewModelPresenterMediatorProvider.Get(typeof(TestView1), true, (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestView1>()), 3,
                wrapperManager);
            Presenter.AddComponent(c2);
            Presenter.AddComponent(c1);

            var list = Presenter.TryShow(isRawRequest ? viewModel : (object)request, DefaultCancellationToken, Metadata);
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView1>>();

            mediators.Clear();
            showCount = 0;
            Presenter.RemoveComponent(c2);
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = Presenter.TryShow(isRawRequest ? viewModel : (object)request, DefaultCancellationToken, Metadata);
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestViewBase>>();

            mediators.Clear();
            showCount = 0;
            Presenter.RemoveComponent(c1);
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = Presenter.TryShow(isRawRequest ? viewModel : (object)request, DefaultCancellationToken, Metadata);
            mediators.Count.ShouldEqual(0);
            list.Count.ShouldEqual(0);
            showCount.ShouldEqual(0);

            var canWrapCount = 0;
            wrapperManager.AddComponent(new DelegateWrapperManager<Type, object>((type, r, m) =>
            {
                ++canWrapCount;
                type.ShouldEqual(typeof(ViewModelPresenterTest));
                r.ShouldEqual(mapping.ViewType);
                m.ShouldEqual(Metadata);
                return true;
            }, (_, _, _) => null!));
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = Presenter.TryShow(isRawRequest ? viewModel : (object)request, DefaultCancellationToken, Metadata);
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            canWrapCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView2>>();
        }

        protected override IPresenter GetPresenter() => new Presenter(ComponentCollectionManager);

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

        private sealed class TestViewModelPresenterMediator<T> : TestViewModelPresenterMediator
        {
        }

        private class TestViewBase
        {
        }

        private class TestView1 : TestViewBase
        {
        }

        private class TestView2 : TestView1
        {
        }
    }
}