using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Extensions;
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
    public class ViewModelViewProviderInitializerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryInitializeAsyncShouldUpdateViewViewModel()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var view = new object();
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult());
            var initializeCount = 0;
            var mapping = new ViewModelViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewInitializerComponent
            {
                TryInitializeAsync = (viewMapping, v, vm, m, token) =>
                {
                    ++initializeCount;
                    viewManager.ShouldEqual(viewManager);
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
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
                TryGetViewModel = (o, type, arg3) =>
                {
                    var request = (ViewModelProviderRequest) o;
                    request.Type.ShouldEqual(viewModelType);
                    return viewModel;
                }
            });

            var component = new ViewModelViewProviderInitializerComponent(viewModelManager, testServiceProvider);
            viewManager.AddComponent(component);

            viewManager.InitializeAsync(mapping, null, null, DefaultMetadata, cancellationToken).ShouldEqual(result);
            initializeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryCleanupAsyncShouldBeHandledByComponents()
        {
            var viewType = typeof(object);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewModelViewMapping("id", viewType, viewModelType, DefaultMetadata);
            var view = new View(mapping, new object());
            var viewModel = new TestViewModel();
            var result = Task.FromResult(new ViewInitializationResult());
            var invokeCount = 0;
            var cancellationToken = new CancellationTokenSource().Token;

            var viewManager = new ViewManager();
            viewManager.AddComponent(new TestViewInitializerComponent
            {
                TryCleanupAsync = (v, vm, meta, token) =>
                {
                    ++invokeCount;
                    v.ShouldEqual(view);
                    vm.ShouldEqual(viewModel);
                    meta.ShouldEqual(DefaultMetadata);
                    token.ShouldEqual(cancellationToken);
                    return result;
                }
            });

            var component = new ViewModelViewProviderInitializerComponent();
            viewManager.AddComponent(component);

            viewManager.CleanupAsync(view, viewModel, DefaultMetadata, cancellationToken).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}