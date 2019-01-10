using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Navigation
{
    public class NavigationContext : INavigationContext
    {
        #region Fields

        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        public NavigationContext(object navigationProvider, NavigationType navigationType, NavigationMode navigationMode,
            IViewModel? viewModelFrom, IViewModel? viewModelTo, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(navigationProvider, nameof(navigationProvider));
            Should.NotBeNull(navigationType, nameof(navigationType));
            NavigationProvider = navigationProvider;
            NavigationType = navigationType;
            NavigationMode = navigationMode;
            ViewModelFrom = viewModelFrom;
            ViewModelTo = viewModelTo;
            if (metadata != null)
            {
                if (metadata is IMetadataContext m)
                    _metadata = m;
                else
                    _metadata = new MetadataContext(metadata);
            }
        }

        #endregion

        #region Properties

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    MugenExtensions.LazyInitialize(ref _metadata, new MetadataContext());
                return _metadata;
            }
        }

        public NavigationMode NavigationMode { get; }

        public NavigationType NavigationType { get; }

        public object NavigationProvider { get; }

        public IViewModel? ViewModelFrom { get; }

        public IViewModel? ViewModelTo { get; }

        #endregion
    }
}