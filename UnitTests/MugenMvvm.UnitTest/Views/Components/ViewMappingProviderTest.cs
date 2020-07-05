using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views.Components
{
    public class ViewMappingProviderTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping1(string name, bool shouldFail)
        {
            var component = new ViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            component.AddMapping(vmType, vType, true, name, name, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                component.TryGetMappings(null!, new ViewModelImpl(), metadata).AsList().ShouldBeEmpty();
                component.TryGetMappings(null!, typeof(ViewModelImpl), metadata).AsList().ShouldBeEmpty();

                var view = new BaseView();
                mapping = component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.FullName + vType.FullName);

                component.TryGetMappings(null!, new ViewImpl(), metadata).AsList().ShouldBeEmpty();
                component.TryGetMappings(null!, typeof(ViewImpl), metadata).AsList().ShouldBeEmpty();
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
            var component = new ViewMappingProvider();
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
                var mapping = component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                mapping = component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                mapping = component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                mapping = component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, id, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), metadata).AsList().Single();
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

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping3(string name, bool shouldFail)
        {
            var component = new ViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var id = $"{vmType.FullName}{vType.FullName}{name}";
            component.AddMapping(vmType, vType, false, name, null, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                var mapping = component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = component.TryGetMappings(null!, view, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(null, view), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = component.TryGetMappings(null!, view, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(null, view), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), DefaultMetadata).AsList().Single();
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
            var component = new ViewMappingProvider();
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            component.AddMapping(vmType, vType, false, null, null, DefaultMetadata);

            var vm = new TestViewModel();
            component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single().ShouldNotBeNull();

            component.ClearMappings();
            component.TryGetMappings(null!, vm, DefaultMetadata).AsList().ShouldBeEmpty();
        }

        #endregion

        #region Nested types

        public class ViewModelImpl : TestViewModel
        {
        }

        public class BaseView : MetadataOwnerBase
        {
            #region Constructors

            public BaseView() : base(null, null)
            {
            }

            #endregion
        }

        public class ViewImpl : BaseView
        {
        }

        #endregion
    }
}