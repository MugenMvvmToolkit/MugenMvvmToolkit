using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Requests;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.Presenters.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters.Components
{
    public class ViewModelMediatorPresenterTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryShowShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            var presenter = new ViewModelMediatorPresenter();
            presenter.TryShow(vm, default, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            presenter.TryShow(this, default, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Fact]
        public void TryCloseShouldIgnoreNoMediators()
        {
            var vm = new TestViewModel();
            var presenter = new ViewModelMediatorPresenter();
            presenter.TryClose(vm, default, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            presenter.TryClose(this, default, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryShowShouldUseRegisteredMediators(bool isRawRequest)
        {
            var viewModel = new TestViewModel();
            var request = new ViewModelViewRequest(viewModel, new object());
            var viewManager = new ViewManager();
            var cancellationToken = new CancellationTokenSource().Token;
            var mapping = new ViewModelViewMapping("t", typeof(TestView1), typeof(TestViewModel), DefaultMetadata);
            viewManager.AddComponent(new TestViewModelViewMappingProviderComponent
            {
                TryGetMappings = (o, type, m) =>
                {
                    if (isRawRequest)
                    {
                        o.ShouldEqual(viewModel);
                        type.ShouldEqual(typeof(TestViewModel));
                    }
                    else
                    {
                        o.ShouldEqual(request);
                        type.ShouldEqual(typeof(ViewModelViewRequest));
                    }

                    m.ShouldEqual(DefaultMetadata);
                    return mapping;
                }
            });

            var initializeCount = 0;
            var showCount = 0;
            var mediators = new List<TestViewModelPresenterMediator>();
            var serviceProvider = new TestServiceProvider
            {
                GetService = type =>
                {
                    var instance = (TestViewModelPresenterMediator)Activator.CreateInstance(type);
                    instance.Initialize = (vm, viewMapping, m) =>
                    {
                        ++initializeCount;
                        viewModel.ShouldEqual(vm);
                        viewMapping.ShouldEqual(mapping);
                        m.ShouldEqual(DefaultMetadata);
                    };
                    instance.TryShow = (o, token, m) =>
                    {
                        ++showCount;
                        o.ShouldEqual(isRawRequest ? null : request.View);
                        token.ShouldEqual(cancellationToken);
                        m.ShouldEqual(DefaultMetadata);
                        return new PresenterResult(viewModel, mapping.Id, instance, NavigationType.Popup);
                    };
                    mediators.Add(instance);
                    return instance;
                }
            };
            var wrapperManager = new WrapperManager();

            var presenter = new ViewModelMediatorPresenter(viewManager, wrapperManager, serviceProvider);
            presenter.RegisterMediator(typeof(TestViewModelPresenterMediator<TestView2>), typeof(TestView2), true, 0);
            presenter.RegisterMediator(typeof(TestViewModelPresenterMediator<TestView2>), typeof(ViewModelMediatorPresenterTest), false, 1);
            var t2 = presenter.RegisterMediator(typeof(TestViewModelPresenterMediator<TestViewBase>), typeof(TestViewBase), false, 2);
            var t1 = presenter.RegisterMediator(typeof(TestViewModelPresenterMediator<TestView1>), typeof(TestView1), true, 3);

            var list = isRawRequest ? presenter.TryShow(viewModel, cancellationToken, DefaultMetadata).AsList() : presenter.TryShow(request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView1>>();

            mediators.Clear();
            initializeCount = 0;
            showCount = 0;
            list = isRawRequest ? presenter.TryShow(viewModel, cancellationToken, DefaultMetadata).AsList() : presenter.TryShow(request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(0);
            list.Count.ShouldEqual(1);
            initializeCount.ShouldEqual(0);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView1>>();

            mediators.Clear();
            initializeCount = 0;
            showCount = 0;
            t1.Dispose();
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = isRawRequest ? presenter.TryShow(viewModel, cancellationToken, DefaultMetadata).AsList() : presenter.TryShow(request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestViewBase>>();

            mediators.Clear();
            initializeCount = 0;
            showCount = 0;
            t2.Dispose();
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = isRawRequest ? presenter.TryShow(viewModel, cancellationToken, DefaultMetadata).AsList() : presenter.TryShow(request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(0);
            list.Count.ShouldEqual(0);
            initializeCount.ShouldEqual(0);
            showCount.ShouldEqual(0);

            int canWrapCount = 0;
            wrapperManager.AddComponent(new DelegateWrapperManager<Type, object, object>((type, r, rt, m) =>
            {
                ++canWrapCount;
                type.ShouldEqual(typeof(ViewModelMediatorPresenterTest));
                r.ShouldEqual(mapping.ViewType);
                m.ShouldEqual(DefaultMetadata);
                return true;
            }, (type, o, arg3, arg4) => null!, null));
            viewModel = new TestViewModel();
            request = new ViewModelViewRequest(viewModel, request.View);
            list = isRawRequest ? presenter.TryShow(viewModel, cancellationToken, DefaultMetadata).AsList() : presenter.TryShow(request, cancellationToken, DefaultMetadata).AsList();
            mediators.Count.ShouldEqual(1);
            list.Count.ShouldEqual(1);
            initializeCount.ShouldEqual(1);
            showCount.ShouldEqual(1);
            canWrapCount.ShouldEqual(1);
            list[0].NavigationProvider.ShouldBeType<TestViewModelPresenterMediator<TestView2>>();
        }

        [Fact]
        public void TryCloseShouldUseRegisteredMediators()
        {
            var cancellationToken = new CancellationTokenSource().Token;
            var viewModel = new TestViewModel();
            var viewManager = new ViewManager();
            var result = new PresenterResult(viewModel, "t", Default.NavigationProvider, NavigationType.Popup);
            int closeCount = 0;
            var mediator = new TestViewModelPresenterMediator
            {
                TryClose = (token, context) =>
                {
                    ++closeCount;
                    token.ShouldEqual(cancellationToken);
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var mapping = new ViewModelViewMapping("t", typeof(object), typeof(object), DefaultMetadata);
            viewManager.AddComponent(new TestViewModelViewMappingProviderComponent
            {
                TryGetMappings = (o, type, arg3) => mapping,
            });
            var wrapperManager = new WrapperManager();
            var serviceProvider = new TestServiceProvider
            {
                GetService = type => mediator
            };

            var presenter = new ViewModelMediatorPresenter(viewManager, wrapperManager, serviceProvider);
            presenter.RegisterMediator(typeof(TestViewModelPresenterMediator), typeof(object), false);

            presenter.TryShow(viewModel, cancellationToken, DefaultMetadata);
            presenter.TryClose(viewModel, cancellationToken, DefaultMetadata).AsList().Single().ShouldEqual(result);
            closeCount.ShouldEqual(1);
        }

        #endregion

        #region Nested types

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

        #endregion
    }
}