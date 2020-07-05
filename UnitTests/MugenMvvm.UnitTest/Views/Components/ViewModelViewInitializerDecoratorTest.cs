using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Requests;
using MugenMvvm.UnitTest.Internal.Internal;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.UnitTest.Views.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewModelViewInitializerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryInitializeAsyncShouldUpdateViewViewModel()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var view = new object();
            var viewModel = new TestViewModel();
            var result = Task.FromResult<IView>(new View(new ViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel()));
            var initializeCount = 0;
            var mapping = new ViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryInitializeAsync = (viewMapping, r, t, m, token) =>
                {
                    ++initializeCount;
                    viewManager.ShouldEqual(viewManager);
                    var request = (ViewModelViewRequest)r;
                    request.View.ShouldEqual(view);
                    request.ViewModel.ShouldEqual(viewModel);
                    m.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });
            var testServiceProvider = new TestServiceProvider
            {
                GetService = type =>
                {
                    type.ShouldEqual(viewType);
                    return view;
                }
            };
            var viewModelManager = new ViewModelManager();
            viewModelManager.AddComponent(new TestViewModelProviderComponent
            {
                TryGetViewModel = (m, o, type, arg3) =>
                {
                    o.ShouldEqual(viewModelType);
                    type.ShouldEqual(typeof(Type));
                    return viewModel;
                }
            });

            var component = new ViewModelViewInitializerDecorator(viewModelManager, testServiceProvider);
            viewManager.AddComponent(component);

            viewManager.InitializeAsync(mapping, new ViewModelViewRequest(), cancellationToken, DefaultMetadata).ShouldEqual(result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            viewManager.InitializeAsync(mapping, viewModel, cancellationToken, DefaultMetadata).ShouldEqual(result);
            initializeCount.ShouldEqual(1);

            initializeCount = 0;
            viewManager.InitializeAsync(mapping, view, cancellationToken, DefaultMetadata).ShouldEqual(result);
            initializeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryCleanupAsyncShouldBeHandledByComponents()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var view = new View(mapping, new object(), new TestViewModel());
            var viewModel = new TestViewModel();
            var result = Task.FromResult(this);
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewManagerComponent
            {
                TryCleanupAsync = (v, r, t, meta, token) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(view);
                    r.ShouldEqual(viewModel);
                    t.ShouldEqual(viewModel.GetType());
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new ViewModelViewInitializerDecorator();
            viewManager.AddComponent(component);

            viewManager.CleanupAsync(view, viewModel, cancellationToken, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}