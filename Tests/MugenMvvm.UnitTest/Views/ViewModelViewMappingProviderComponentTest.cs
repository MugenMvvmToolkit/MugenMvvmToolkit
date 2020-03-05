using System.Linq;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.ViewModels;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewModelViewMappingProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping1(string name, bool shouldFail)
        {
            var component = new ViewModelViewMappingProviderComponent();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            component.AddMapping(vmType, vType, true, name, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = component.TryGetMappingByViewModel(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                component.TryGetMappingByViewModel(new ViewModelImpl(), metadata).ShouldBeNull();

                var view = new BaseView();
                mapping = component.TryGetMappingByView(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                component.TryGetMappingByView(new ViewImpl(), metadata).ShouldBeNull();
            }
            catch
            {
                if (!shouldFail)
                    throw;
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping2(string name, bool shouldFail)
        {
            var component = new ViewModelViewMappingProviderComponent();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            component.AddMapping(vmType, vType, false, name, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = component.TryGetMappingByViewModel(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                mapping = component.TryGetMappingByViewModel(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                mapping = component.TryGetMappingByView(view, metadata).Single();
                mapping.ViewType.ShouldEqual(view.GetType());
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                mapping = component.TryGetMappingByView(view, metadata).Single();
                mapping.ViewType.ShouldEqual(view.GetType());
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
            }
            catch
            {
                if (!shouldFail)
                    throw;
            }
        }

        [Fact]
        public void AddMappingShouldAddMapping3()
        {
            var component = new ViewModelViewMappingProviderComponent();
            var vm = new TestViewModel();
            var view = new BaseView();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            int getViewCount = 0;
            int getViewModelCount = 0;
            component.AddMapping((viewModel, context) =>
            {
                ++getViewCount;
                viewModel.ShouldEqual(vm);
                context.ShouldEqual(DefaultMetadata);
                return vType;
            }, (v, context) =>
            {
                ++getViewModelCount;
                view.ShouldEqual(v);
                context.ShouldEqual(DefaultMetadata);
                return vmType;
            }, DefaultMetadata);

            component.TryGetMappingByView(view, DefaultMetadata).Single().ViewType.ShouldEqual(vType);
            getViewCount.ShouldEqual(0);
            getViewModelCount.ShouldEqual(1);

            component.TryGetMappingByViewModel(vm, DefaultMetadata).Single().ViewModelType.ShouldEqual(vmType);
            getViewCount.ShouldEqual(1);
            getViewModelCount.ShouldEqual(1);
        }

        [Fact]
        public void ClearMappingsShouldClearMappings()
        {
            var component = new ViewModelViewMappingProviderComponent();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            component.AddMapping(vmType, vType, false, null, DefaultMetadata);

            var vm = new TestViewModel();
            component.TryGetMappingByViewModel(vm, DefaultMetadata).Single().ShouldNotBeNull();

            component.ClearMappings();
            component.TryGetMappingByViewModel(vm, DefaultMetadata).ShouldBeNull();
        }

        #endregion

        #region Nested types

        public class ViewModelImpl : TestViewModel
        {
        }

        public class BaseView
        {
        }

        public class ViewImpl : BaseView
        {
        }

        #endregion
    }
}