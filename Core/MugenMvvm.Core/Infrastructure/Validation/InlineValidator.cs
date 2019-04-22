using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Infrastructure.Validation
{
    public sealed class InlineValidator<T> : ValidatorBase<T> where T : class
    {
        #region Constructors

        public InlineValidator(IObservableMetadataContext metadata = null, IComponentCollectionProvider componentCollectionProvider = null)
            : base(metadata, componentCollectionProvider, false)
        {
        }

        #endregion

        #region Methods

        public void SetErrors(string memberName, params object[] errors)
        {
            SetErrors(memberName, Default.MetadataContext, errors);
        }

        public void SetErrors(string memberName, IReadOnlyMetadataContext metadata, params object[] errors)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            UpdateErrors(memberName, errors, false, metadata);
        }

        protected override Task<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext metadata)
        {
            return ValidationResult.DoNothingTask;
        }

        #endregion
    }
}