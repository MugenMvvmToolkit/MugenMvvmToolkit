using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelComponentOwner : TestViewModel, IComponentOwner<IViewModelBase>
    {
        private IComponentCollection? _components;

        public TestViewModelComponentOwner(IComponentCollection? components = null, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            _components = components;
        }

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? MugenService.ComponentCollectionManager.EnsureInitialized(ref _components, this);
    }
}