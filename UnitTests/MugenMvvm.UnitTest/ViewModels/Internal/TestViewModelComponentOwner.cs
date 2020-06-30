using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModelComponentOwner : TestViewModel, IComponentOwner<IViewModelBase>
    {
        #region Fields

        private IComponentCollection? _components;

        #endregion

        #region Constructors

        public TestViewModelComponentOwner(IComponentCollection? components = null, IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(metadata, metadataContextManager)
        {
            _components = components;
        }

        #endregion

        #region Properties

        public bool HasComponents => _components != null && _components.Count != 0;

        public IComponentCollection Components
        {
            get
            {
                if (_components == null)
                    MugenService.ComponentCollectionManager.LazyInitialize(ref _components, this);
                return _components;
            }
        }

        #endregion
    }
}