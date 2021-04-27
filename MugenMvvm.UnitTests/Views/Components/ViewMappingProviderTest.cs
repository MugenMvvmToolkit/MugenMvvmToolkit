using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Views.Components
{
    public class ViewMappingProviderTest : UnitTestBase
    {
        private readonly ViewMappingProvider _component;

        public ViewMappingProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _component = new ViewMappingProvider();
        }

        [Fact]
        public void ClearMappingsShouldClearMappings()
        {
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            _component.AddMapping(vmType, vType, false, null, null, DefaultMetadata);

            var vm = new TestViewModel();
            _component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single().ShouldNotBeNull();

            _component.ClearMappings();
            _component.TryGetMappings(null!, vm, DefaultMetadata).AsList().ShouldBeEmpty();
        }

        [Fact]
        public void ShouldSupportGenericViewModelMappings1()
        {
            var mapping = _component.AddMapping(typeof(GenericViewModelBase<>), typeof(BaseView), true);
            var vm = new GenericViewModelBase<object>();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);

            vm = new GenericViewModel();
            _component.TryGetMappings(null!, vm, null).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportGenericViewModelMappings2()
        {
            var mapping = _component.AddMapping(typeof(GenericViewModelBase<>), typeof(BaseView), false);
            var vm = new GenericViewModelBase<object>();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);

            vm = new GenericViewModel();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);
        }

        [Fact]
        public void ShouldSupportGenericViewMappings1()
        {
            var mapping = _component.AddMapping(typeof(ViewModelImpl), typeof(GenericViewBase<>), true);
            var vm = new GenericViewBase<object>();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);

            vm = new GenericView();
            _component.TryGetMappings(null!, vm, null).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportGenericViewMappings2()
        {
            var mapping = _component.AddMapping(typeof(ViewModelImpl), typeof(GenericViewBase<>), false);
            var vm = new GenericViewBase<object>();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);

            vm = new GenericView();
            _component.TryGetMappings(null!, vm, null).Item.ShouldEqual(mapping);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping1(string name, bool shouldFail)
        {
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            _component.AddMapping(vmType, vType, true, name, name, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = _component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = _component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                _component.TryGetMappings(null!, new ViewModelImpl(), metadata).AsList().ShouldBeEmpty();
                _component.TryGetMappings(null!, typeof(ViewModelImpl), metadata).AsList().ShouldBeEmpty();

                var view = new BaseView();
                mapping = _component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = _component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                _component.TryGetMappings(null!, new ViewImpl(), metadata).AsList().ShouldBeEmpty();
                _component.TryGetMappings(null!, typeof(ViewImpl), metadata).AsList().ShouldBeEmpty();
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
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var id = $"{vmType.Name}{vType.Name}{name}";
            var metadata = new MetadataContext();
            if (name != null && !shouldFail)
                metadata.Set(NavigationMetadata.ViewName, name);
            _component.AddMapping(vmType, vType, false, name, null, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = _component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                mapping = _component.TryGetMappings(null!, vm, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, vm.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                mapping = _component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                mapping = _component.TryGetMappings(null!, view, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(null, view), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, view.GetType(), metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, id, metadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), metadata).AsList().Single();
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
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            var id = $"{vmType.Name}{vType.Name}{name}";
            _component.AddMapping(vmType, vType, false, name, null, DefaultMetadata);

            try
            {
                var vm = new TestViewModel();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                var mapping = _component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = _component.TryGetMappings(null!, vm, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, null), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = _component.TryGetMappings(null!, view, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(null, view), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = _component.TryGetMappings(null!, view, DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(null, view), DefaultMetadata).AsList().Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = _component.TryGetMappings(null!, new ViewModelViewRequest(vm, view), DefaultMetadata).AsList().Single();
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

        public class ViewModelImpl : TestViewModel
        {
        }

        public class BaseView : MetadataOwnerBase
        {
            public BaseView() : base(null)
            {
            }
        }

        public class ViewImpl : BaseView
        {
        }

        public class GenericViewModelBase<T> : TestViewModel
        {
        }

        public class GenericViewModel : GenericViewModelBase<object>
        {
        }

        public class GenericViewBase<T>
        {
        }

        public class GenericView : GenericViewBase<object>
        {
        }
    }
}