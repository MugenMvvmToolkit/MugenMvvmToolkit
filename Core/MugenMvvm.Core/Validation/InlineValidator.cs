using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Validation
{
    public sealed class InlineValidator : ValidatorBase<object>
    {
        #region Constructors

        public InlineValidator(IReadOnlyMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(false, metadata, componentCollectionProvider, metadataContextProvider)
        {
            Initialize(this);
        }

        #endregion

        #region Methods

        public void SetErrors(string memberName, params object[] errors)
        {
            SetErrors(memberName, errors, null);
        }

        public void SetErrors(string memberName, IReadOnlyList<object>? errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            UpdateErrors(memberName, errors, false, metadata);
        }

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return default;
        }

        #endregion
    }
}