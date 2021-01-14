using MugenMvvm.Busy;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    public class InheritParentViewModelServiceResolverTest : UnitTestBase
    {
        private readonly ViewModelManager _viewModelManager;

        public InheritParentViewModelServiceResolverTest()
        {
            _viewModelManager = new ViewModelManager();
            _viewModelManager.AddComponent(new InheritParentViewModelServiceResolver());
            _viewModelManager.AddComponent(new TestViewModelServiceResolverComponent
            {
                TryGetService = (_, o, _) =>
                {
                    if (typeof(IMetadataContext).Equals(o))
                        return new MetadataContext();
                    return null;
                }
            });
        }

        [Fact]
        public void TryGetServiceShouldIgnoreNonTypeService()
        {
            var vm = new TestViewModelBase(_viewModelManager);
            _viewModelManager.TryGetService(vm, this).ShouldBeNull();
        }

        [Fact]
        public void TryGetServiceShouldIgnoreServiceWithoutParentVm()
        {
            var vm = new TestViewModelBase(_viewModelManager);
            _viewModelManager.TryGetService(vm, typeof(IBusyManager)).ShouldBeNull();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetServiceShouldGetServiceFromParentVm(bool vmMetadata)
        {
            var messenger = new Messenger();
            var busyManager = new BusyManager();
            using var t = _viewModelManager.AddComponent(new TestViewModelServiceResolverComponent
            {
                TryGetService = (_, o, _) =>
                {
                    if (typeof(IMessenger).Equals(o))
                        return messenger;
                    if (typeof(IBusyManager).Equals(o))
                        return busyManager;
                    return null;
                }
            });
            var parentVm = new TestViewModelBase(_viewModelManager);
            var vm = new TestViewModelBase(_viewModelManager);
            if (vmMetadata)
                vm.Metadata.Set(ViewModelMetadata.ParentViewModel, parentVm);
            var meta = vmMetadata ? DefaultMetadata : ViewModelMetadata.ParentViewModel.ToContext(parentVm);

            parentVm.Messenger.ShouldEqual(messenger);
            parentVm.BusyManager.ShouldEqual(busyManager);
            _viewModelManager.TryGetService(vm, typeof(IMessenger), meta).ShouldEqual(messenger);
            _viewModelManager.TryGetService(vm, typeof(IBusyManager), meta).ShouldEqual(busyManager);
        }
    }
}