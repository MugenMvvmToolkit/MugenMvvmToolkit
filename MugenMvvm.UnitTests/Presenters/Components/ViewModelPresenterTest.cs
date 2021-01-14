using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Navigation;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.Presenters.Internal;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presenters.Components
{
    public class ViewModelPresenterTest : UnitTestBase
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldUseRegisteredMediators(bool isRawRequest)
        {
            var viewModel = new TestViewModel();
            var request = new ViewModelViewRequest(viewModel, new object());
            var viewManager = new ViewManager();
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewMapping("t", typeof(TestViewModel), typeof(TestView1), DefaultMetadata);
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, m) =>
                {
                    if (isRawRequest)
                        o.ShouldEqual(viewModel);
                    else
                        o.ShouldEqual(request);

                    m.ShouldEqual(DefaultMetadata);
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
                    token.ShouldEqual(cancellationToken);
                    m.ShouldEqual(DefaultMetadata);
                    return new PresenterResult(viewModel!, mapping!.Id, instance, NavigationType.Popup);
                };
                mediators!.Add(instance);
                return instance;
            }

            var presenter = new Presenter();
            var wrapperManager = new WrapperManager();
            var viewModelPresenter = new ViewModelPresenter(viewManager);
            presenter.AddComponent(ViewModelPresenterMediatorProvider.Get((p, vm, m, meta) =>
            {
                meta.ShouldEqual(DefaultMetadata);
                if (m.ViewType == typeof(TestView2))
                    return Initialize(new TestViewModelPresenterMediator<TestView2>());
                return null;
            }));
            presenter.AddComponent(ViewModelPresenterMediatorProvider.Get(typeof(ViewModelPresenterTest), false,
                (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestView2>()), 1, wrapperManager));
            var c1 = ViewModelPresenterMediatorProvider.Get(typeof(TestViewBase), false, (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestViewBase>()), 2,
                wrapperManager);
            var c2 = ViewModelPresenterMediatorProvider.Get(typeof(TestView1), true, (p, vm, m, meta) => Initialize(new TestViewModelPresenterMediator<TestView1>()), 3,
                wrapperManager);
            presenter.AddComponent(c2);
            presenter.AddComponent(c1);

            var list = viewModelPresenter.TryShow(presenter, isRawRequest ? viewModel : (object) request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView1>>();

            mediators.Clear();
            showCount = 0;
            presenter.RemoveComponent(c2);
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = viewModelPresenter.TryShow(presenter, isRawRequest ? viewModel : (object) request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestViewBase>>();

            mediators.Clear();
            showCount = 0;
            presenter.RemoveComponent(c1);
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = viewModelPresenter.TryShow(presenter, isRawRequest ? viewModel : (object) request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(0);
            list.Count.ShouldEqual(0);
            showCount.ShouldEqual(0);

            var canWrapCount = 0;
            wrapperManager.AddComponent(new DelegateWrapperManager<Type, object>((type, r, m) =>
            {
                ++canWrapCount;
                type.ShouldEqual(typeof(ViewModelPresenterTest));
                r.ShouldEqual(mapping.ViewType);
                m.ShouldEqual(DefaultMetadata);
                return true;
            }, (type, o, arg4) => null!));
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = viewModelPresenter.TryShow(presenter, isRawRequest ? viewModel : (object) request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            showCount.ShouldEqual(1);
            canWrapCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView2>>();
        }

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

        [Fact]
        public void TryCloseShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            var presenter = new Presenter();
            var viewModelPresenter = new ViewModelPresenter();
            viewModelPresenter.TryClose(presenter, vm, default, DefaultMetadata).IsEmpty.ShouldBeTrue();
            viewModelPresenter.TryClose(presenter, this, default, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryCloseShouldUseRegisteredMediators()
        {
            var cancellationToken = new CancellationTokenSource().Token;
            var viewModel = new TestViewModel();
            var view = new object();
            var request = new ViewModelViewRequest(viewModel, view);
            var viewManager = new ViewManager();
            var result = new PresenterResult(viewModel, "t", NavigationProvider.System, NavigationType.Popup);
            var closeCount = 0;
            var mediator = new TestViewModelPresenterMediator
            {
                TryClose = (v, token, context) =>
                {
                    ++closeCount;
                    v.ShouldEqual(request.View);
                    token.ShouldEqual(cancellationToken);
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var mapping = new ViewMapping("t", typeof(object), typeof(object), DefaultMetadata);
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, arg3) => mapping
            });

            var presenter = new Presenter();
            var viewModelPresenter = new ViewModelPresenter(viewManager);
            presenter.AddComponent(ViewModelPresenterMediatorProvider.Get(typeof(object), false, (p, vm, m, meta) => mediator));

            viewModelPresenter.TryShow(presenter, viewModel, cancellationToken, DefaultMetadata);
            viewModelPresenter.TryClose(presenter, request, cancellationToken, DefaultMetadata).AsList().Single().ShouldEqual(result);
            closeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryShowShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            var presenter = new Presenter();
            var viewModelPresenter = new ViewModelPresenter();
            viewModelPresenter.TryShow(presenter, vm, default, DefaultMetadata).IsEmpty.ShouldBeTrue();
            viewModelPresenter.TryShow(presenter, this, default, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }
    }
}