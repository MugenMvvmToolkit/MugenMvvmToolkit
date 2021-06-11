using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Messaging;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.ViewModels.Components
{
    [Collection(SharedContext)]
    public class InheritParentViewModelServiceProviderTest : UnitTestBase
    {
        public InheritParentViewModelServiceProviderTest()
        {
            var component = new InheritParentViewModelServiceProvider
            {
                ServiceMapping =
                {
                    [typeof(IBusyManager)] = InheritParentViewModelServiceProvider.GetService<IBusyManager>,
                    [typeof(IMessenger)] = InheritParentViewModelServiceProvider.GetService<IMessenger>
                }
            };
            ViewModelManager.AddComponent(component);
            ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (v, _, o, _) =>
                {
                    v.ShouldEqual(ViewModelManager);
                    if (typeof(IMetadataContext).Equals(o))
                        return new MetadataContext();
                    return null;
                }
            });
            RegisterDisposeToken(WithGlobalService(WeakReferenceManager));
        }

        [Fact]
        public void TryGetServiceShouldIgnoreNonTypeService()
        {
            var vm = new TestViewModelBase(ViewModelManager);
            ViewModelManager.TryGetService(vm, this).ShouldBeNull();
        }

        [Fact]
        public void TryGetServiceShouldIgnoreServiceWithoutParentVm()
        {
            var vm = new TestViewModelBase(ViewModelManager);
            ViewModelManager.TryGetService(vm, typeof(IBusyManager)).ShouldBeNull();
        }

        protected override IViewModelManager GetViewModelManager() => new ViewModelManager(ComponentCollectionManager);

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TryGetServiceShouldGetServiceFromParentVm(bool vmMetadata)
        {
            var messenger = new Messenger(ComponentCollectionManager);
            using var t = ViewModelManager.AddComponent(new TestViewModelServiceProviderComponent
            {
                TryGetService = (v, _, o, _) =>
                {
                    v.ShouldEqual(ViewModelManager);
                    if (typeof(IMessenger).Equals(o))
                        return messenger;
                    if (typeof(IBusyManager).Equals(o))
                        return BusyManager;
                    return null;
                }
            });
            var parentVm = new TestViewModelBase(ViewModelManager);
            var vm = new TestViewModelBase(ViewModelManager);
            if (vmMetadata)
                vm.Metadata.Set(ViewModelMetadata.ParentViewModel, parentVm);
            var meta = vmMetadata ? DefaultMetadata : ViewModelMetadata.ParentViewModel.ToContext(parentVm);

            parentVm.Messenger.ShouldEqual(messenger);
            parentVm.BusyManager.ShouldEqual(BusyManager);
            ViewModelManager.TryGetService(vm, typeof(IMessenger), meta).ShouldEqual(messenger);
            ViewModelManager.TryGetService(vm, typeof(IBusyManager), meta).ShouldEqual(BusyManager);
        }
    }
}