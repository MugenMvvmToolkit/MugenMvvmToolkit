using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.ViewModels
{
    public class TestViewModel : MetadataOwnerBase, IViewModelBase
    {
        #region Constructors

        public TestViewModel(IReadOnlyMetadataContext? metadata = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, metadataContextProvider)
        {
        }

        #endregion
    }
}