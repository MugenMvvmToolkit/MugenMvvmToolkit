using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Requests;

namespace MugenMvvm.Requests
{
    public class StateRequest : CancelableRequest, IStateRequest
    {
        #region Fields

        private readonly IMetadataContextManager? _metadataContextManager;
        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public StateRequest(bool cancel = false, object? state = null, IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
            : base(cancel, state)
        {
            _metadata = metadata;
            _metadataContextManager = metadataContextManager;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadataContextManager.LazyInitializeNonReadonly(ref _metadata, this);

        #endregion
    }
}