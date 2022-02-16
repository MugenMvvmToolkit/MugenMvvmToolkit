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
            _component.AddMapping(vmType, vType, true, name, name, null, Metadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vm.GetType());
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                ViewManager.GetMappings(new ViewModelImpl(), metadata).ShouldBeEmpty();
                ViewManager.GetMappings(typeof(ViewModelImpl), metadata).ShouldBeEmpty();

                var view = new BaseView();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(Metadata);
                mapping.Id.ShouldEqual(name ?? vmType.Name + vType.Name);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Metadata.ShouldEqual(Metadata);
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
            _component.AddMapping(vmType, vType, false, name, null, null, Metadata);

            try
            {
                var vm = new TestViewModel();
                var mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                vm = new ViewModelImpl();
                mapping = ViewManager.GetMappings(vm, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(vm.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                var view = new BaseView();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                view = new ViewImpl();
                mapping = ViewManager.GetMappings(view, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(view.GetType(), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(id, metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);
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
            _component.AddMapping(vmType, vType, false, name, null, null, Metadata);

            try
            {
                var vm = new TestViewModel();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                var mapping = ViewManager.GetMappings(vm, Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                vm = new ViewModelImpl();
                if (name != null && !shouldFail)
                    vm.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(vm, Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, null), Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                var view = new BaseView();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(view, Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                view = new ViewImpl();
                if (name != null && !shouldFail)
                    view.Metadata.Set(NavigationMetadata.ViewName, name);
                mapping = ViewManager.GetMappings(view, Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(null, view), Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);

                mapping = ViewManager.GetMappings(new ViewModelViewRequest(vm, view), Metadata).Single();
                mapping.ViewType.ShouldEqual(vType);
                mapping.ViewModelType.ShouldEqual(vmType);
                mapping.Id.ShouldEqual(id);
                mapping.Metadata.ShouldEqual(Metadata);
            }
            catch
            {
                if (!shouldFail)
                    throw;
            }
        }

        [Fact]
        public void AddMappingShouldSupportPostCondition()
        {
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);

            int invokeCount = 0;
            IViewMapping? expectedMapping = null;
            var expectedType = vmType;
            var expectedIsViewMapping = false;
            object? expectedTarget = null;
            bool result = true;

            expectedMapping = _component.AddMapping(vmType, vType, false, null, null, (m, t, isViewMapping, target, c, metadata) =>
            {
                m.ShouldEqual(expectedMapping);
                expectedType.ShouldEqual(t);
                expectedIsViewMapping.ShouldEqual(isViewMapping);
                target.ShouldEqual(expectedTarget);
                c.ShouldEqual(0);
                metadata.ShouldEqual(Metadata);
                ++invokeCount;
                return result;
            }, Metadata);


            var vm = new TestViewModel();
            expectedTarget = vm;
            expectedType = vm.GetType();
            expectedIsViewMapping = false;
            ViewManager.GetMappings(vm, Metadata).Single().ShouldEqual(expectedMapping);
            invokeCount.ShouldEqual(1);

            var view = new BaseView();
            expectedTarget = view;
            expectedType = view.GetType();
            expectedIsViewMapping = true;
            ViewManager.GetMappings(view, Metadata).Single().ShouldEqual(expectedMapping);
            invokeCount.ShouldEqual(2);
        }

        [Fact]
        public void ClearMappingsShouldClearMappings()
        {
            var vmType = typeof(TestViewModel);
            var vType = typeof(BaseView);
            _component.AddMapping(vmType, vType, false, null, null, null, Metadata);

            var vm = new TestViewModel();
            ViewManager.GetMappings(vm, Metadata).Single().ShouldNotBeNull();

            _component.ClearMappings();
            ViewManager.GetMappings(vm, Metadata).ShouldBeEmpty();
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