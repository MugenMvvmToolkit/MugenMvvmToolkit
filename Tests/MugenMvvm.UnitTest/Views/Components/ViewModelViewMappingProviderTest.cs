using System.Linq;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewModelViewMappingProviderTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping1(string name, bool shouldFail)
        {
            var component = new ViewModelViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            component.AddMapping(vmType, vType, true, name, name, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = component.TryGetMappings(vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                component.TryGetMappings(new ViewModelImpl(), metadata).AsList().ShouldBeEmpty();
                component.TryGetMappings(typeof(ViewModelImpl), metadata).AsList().ShouldBeEmpty();

                var view = new BaseView();
                mapping = component.TryGetMappings(view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                component.TryGetMappings(new ViewImpl(), metadata).AsList().ShouldBeEmpty();
                component.TryGetMappings(typeof(ViewImpl), metadata).AsList().ShouldBeEmpty();
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
            var component = new ViewModelViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var id = $"{vmType.FullName}{vType.FullName}{name}";
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            component.AddMapping(vmType, vType, false, name, null, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = component.TryGetMappings(vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                mapping = component.TryGetMappings(vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                mapping = component.TryGetMappings(view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                mapping = component.TryGetMappings(view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(id, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
            }
            catch
            {
                if (!shouldFail)
                    throw;
            }
        }

        [Fact]
        public void ClearMappingsShouldClearMappings()
        {
            var component = new ViewModelViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            component.AddMapping(vmType, vType, false, null, null, DefaultMetadata);

            var vm = new TestViewModel();
            component.TryGetMappings(vm, DefaultMetadata).AsList().Single().ShouldNotBeNull();

            component.ClearMappings();
            component.TryGetMappings(vm, DefaultMetadata).AsList().ShouldBeEmpty();
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