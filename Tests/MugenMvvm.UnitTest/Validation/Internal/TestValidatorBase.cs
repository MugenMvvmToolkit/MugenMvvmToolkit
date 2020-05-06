using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Validation;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorBase<T> : ValidatorBase<T> where T : class
    {
        #region Constructors

        public TestValidatorBase(bool hasAsyncValidation, IReadOnlyMetadataContext? metadata = null, IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
            : base(hasAsyncValidation, metadata, componentCollectionProvider, metadataContextProvider)
        {
        }

        #endregion

        #region Properties

        internal Func<string, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ValidationResult>>? GetErrorsAsyncDelegate { get; set; }

        #endregion

        #region Methods

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            return GetErrorsAsyncDelegate?.Invoke(memberName, cancellationToken, metadata) ?? default;
        }

        #endregion
    }
}