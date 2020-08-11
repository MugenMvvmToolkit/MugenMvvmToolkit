﻿using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Requests;

namespace MugenMvvm.Requests
{
    public class StateRequest : CancelableRequest, IStateRequest
    {
        #region Fields

        private IReadOnlyMetadataContext? _metadata;

        #endregion

        #region Constructors

        public StateRequest(bool? cancel = null, object? state = null, IReadOnlyMetadataContext? metadata = null)
            : base(cancel, state)
        {
            _metadata = metadata;
        }

        #endregion

        #region Properties

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => MugenExtensions.LazyInitialize(ref _metadata);

        #endregion
    }
}