using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Presenters.Results;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Presenters.Results
{
    public class ChildViewModelPresenterResult : IChildViewModelPresenterResult
    {
        #region Constructors

        public ChildViewModelPresenterResult(IReadOnlyMetadataContext metadata, NavigationType navigationType, bool isRestorable)
        {
            Metadata = metadata;
            NavigationType = navigationType;
            IsRestorable = isRestorable;
        }

        #endregion

        #region Properties

        public IReadOnlyMetadataContext Metadata { get; }

        public NavigationType NavigationType { get; }

        public bool IsRestorable { get; }

        #endregion
    }
}