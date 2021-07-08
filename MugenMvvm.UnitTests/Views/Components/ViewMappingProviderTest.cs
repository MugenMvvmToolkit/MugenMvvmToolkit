using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Metadata;
using MugenMvvm.Requests;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.Views;
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
            ViewManager.AddComponent(_component);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("test", false)]
        [InlineData("test", true)]
        public void AddMappingShouldAddMapping1(string? name, bool shouldFail)
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
                var mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                ViewManager.GetMappings(new ViewModelImpl(), metadata).ShouldBeEmpty();
                ViewManager.GetMappings(typeof(ViewModelImpl), metadata).ShouldBeEmpty();

                var view = new BaseView();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(DefaultMetadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                ViewManager.GetMappings(new ViewImpl(), metadata).ShouldBeEmpty();
                ViewManager.GetMappings(typeof(ViewImpl), metadata).ShouldBeEmpty();
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
        public void AddMappingShouldAddMapping2(string? name, bool shouldFail)
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
                var mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(id, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), metadata).Single();
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
        public void AddMappingShouldAddMapping3(string? name, bool shouldFail)
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
                var mapping = ViewManager.GetMappings(vm, DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                vm = new ViewModelImpl();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(vm, DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                var view = new BaseView();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(view, DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                view = new ViewImpl();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(view, DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), DefaultMetadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(DefaultMetadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), DefaultMetadata).Single();
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
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            _component.AddMapping(vmType, vType, false, null, null, DefaultMetadata);

            var vm = new TestViewModel();
            ViewManager.GetMappings(vm, DefaultMetadata).Single().ShouldNotBeNull();

            _component.ClearMappings();
            ViewManager.GetMappings(vm, DefaultMetadata).ShouldBeEmpty();
        }

        [Fact]
        public void ShouldSupportGenericViewMappings1()
        {
            var mapping = _component.AddMapping(typeof(ViewModelImpl), typeof(GenericViewBase<>));
            var vm = new GenericViewBase<object>();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);

            vm = new GenericView();
            ViewManager.GetMappings(vm).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportGenericViewMappings2()
        {
            var mapping = _component.AddMapping(typeof(ViewModelImpl), typeof(GenericViewBase<>), false);
            var vm = new GenericViewBase<object>();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);

            vm = new GenericView();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);
        }

        [Fact]
        public void ShouldSupportGenericViewModelMappings1()
        {
            var mapping = _component.AddMapping(typeof(GenericViewModelBase<>), typeof(BaseView));
            var vm = new GenericViewModelBase<object>();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);

            vm = new GenericViewModel();
            ViewManager.GetMappings(vm).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ShouldSupportGenericViewModelMappings2()
        {
            var mapping = _component.AddMapping(typeof(GenericViewModelBase<>), typeof(BaseView), false);
            var vm = new GenericViewModelBase<object>();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);

            vm = new GenericViewModel();
            ViewManager.GetMappings(vm).Item.ShouldEqual(mapping);
        }

        protected override IViewManager GetViewManager() => new ViewManager(ComponentCollectionManager);

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