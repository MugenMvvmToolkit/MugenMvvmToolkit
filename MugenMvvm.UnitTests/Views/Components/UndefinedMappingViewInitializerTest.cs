using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.UnitTests.Views.Internal;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class UndefinedMappingViewInitializerTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsNotUndefinedMapping()
        {
            var request = new ViewModelViewRequest(new TestViewModel(), typeof(object));
            var mapping = new ViewMapping("id", typeof(IViewModelBase), typeof(object), DefaultMetadata);
            var result = new ValueTask<IView?>();
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(mapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsMoreThatOneMapping()
        {
            var request = new ViewModelViewRequest(new TestViewModel(), typeof(object));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return new[] {new ViewMapping("1", typeof(IViewModelBase), typeof(object)), new ViewMapping("1", typeof(IViewModelBase), typeof(object))};
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(mapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldReturnCreatedView()
        {
            var request = new ViewModelViewRequest(new TestViewModel(), typeof(object));
            var mapping = ViewMapping.Undefined;
            var viewMapping = new ViewMapping("1", typeof(IViewModelBase), typeof(object));
            var result = new View(viewMapping, this, request.ViewModel!);
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return viewMapping;
                }
            });
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(DefaultMetadata);
                    return result;
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) => throw new NotSupportedException()
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping1()
        {
            var request = new ViewModelViewRequest(new TestViewModel(), typeof(object));
            var mapping = ViewMapping.Undefined;
            var newMapping = new ViewMapping("1", typeof(IViewModelBase), typeof(object));
            var result = new ValueTask<IView?>();
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return newMapping;
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(newMapping);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping2()
        {
            var request = new ViewModelViewRequest(new TestViewModel(), typeof(int));
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.Id.ShouldEqual($"a{request.ViewModel!.GetType().FullName}{typeof(int).FullName}");
                    m.ViewModelType.ShouldEqual(request.ViewModel.GetType());
                    m.ViewType.ShouldEqual(typeof(int));
                    m.Metadata.ShouldEqual(DefaultMetadata);
                    request.View.ShouldBeNull();
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsWithNewMapping3()
        {
            var request = new TestViewModel();
            var mapping = ViewMapping.Undefined;
            var newMapping = new ViewMapping("1", typeof(IViewModelBase), typeof(object));
            var result = new ValueTask<IView?>();
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return newMapping;
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(newMapping);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryInitializeAsyncShouldBeHandledByComponentsDifferentView()
        {
            var view = new object();
            var oldView = new object();
            var request = new ViewModelViewRequest(new TestViewModel(), view);
            var mapping = ViewMapping.Undefined;
            var result = new ValueTask<IView?>();
            var viewMapping = new ViewMapping("1", typeof(IViewModelBase), typeof(object));
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewMappingProviderComponent
            {
                TryGetMappings = (o, context) =>
                {
                    o.ShouldEqual(request);
                    context.ShouldEqual(DefaultMetadata);
                    return viewMapping;
                }
            });
            viewManager.AddComponent(new TestViewProviderComponent
            {
                TryGetViews = (o, context) =>
                {
                    o.ShouldEqual(request.ViewModel);
                    context.ShouldEqual(DefaultMetadata);
                    return ItemOrList.FromItem<IView>(new View(viewMapping, oldView, request.ViewModel!));
                }
            });
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (m, r, meta, token) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(viewMapping);
                    request.View.ShouldEqual(view);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            (await viewManager.TryInitializeAsync(mapping, request, cancellationToken, DefaultMetadata)).ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public async Task TryCleanupAsyncShouldBeHandledByComponents()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewMapping("id", viewModelType, viewType, DefaultMetadata);
            var view = new View(mapping, new object(), new TestViewModel());
            var viewModel = new TestViewModel();
            var result = Default.TrueTask;
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, r, meta, token) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(view);
                    r.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new UndefinedMappingViewInitializer();
            viewManager.AddComponent(component);

            var r = await viewManager.TryCleanupAsync(view, viewModel, cancellationToken, DefaultMetadata);
            r.ShouldEqual(result.Result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}