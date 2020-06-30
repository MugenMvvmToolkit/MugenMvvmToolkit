using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.UnitTest.ViewModels.Internal
{
    public class TestViewModel : MetadataOwnerBase, IViewModelBase
    {
        #region Constructors

        public TestViewModel(IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(metadata, metadataContextManager)
        {
        }

        #endregion
    }
}