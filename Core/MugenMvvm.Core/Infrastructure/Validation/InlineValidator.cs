using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Validation
{
    public sealed class InlineValidator<T> : ValidatorBase<T> where T : class
    {
        #region Constructors

        public InlineValidator(IObservableMetadataContext metadata = null, IComponentCollectionProvider componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
            : base(metadata, componentCollectionProvider, metadataContextProvider, false)
        {
        }

        #endregion

        #region Methods

        public void SetErrors(string memberName, params object[] errors)
        {
            SetErrors(memberName, Default.Metadata, errors);
        }

        public void SetErrors(string memberName, IReadOnlyMetadataContext metadata, params object[] errors)
        {
            SetErrors(memberName, errors, metadata);
        }

        public void SetErrors(string memberName, IReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            UpdateErrors(memberName, errors, false, metadata.DefaultIfNull());
        }

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext metadata)
        {
            return ValidationResult.DoNothingTask;
        }

        #endregion
    }
}