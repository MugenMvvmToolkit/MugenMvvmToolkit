using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.UnitTests.ViewModels.Internal
{
    public class TestViewModelComponentOwner : TestViewModel, IComponentOwner<IViewModelBase>
    {
        #region Fields

        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public TestViewModelComponentOwner(IComponentCollection? components = null, IReadOnlyMetadataContext? metadata = null)
            : base(metadata)
        {
            _components = components;
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components => _components ?? MugenService.ComponentCollectionManager.EnsureInitialized(ref _components, this);

        #endregion
    }
}