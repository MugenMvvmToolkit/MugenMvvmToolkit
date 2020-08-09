using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.UnitTest.Validation.Internal
{
    public class TestValidatorComponentBase<T> : ValidatorComponentBase<T> where T : class
    {
        #region Constructors

        public TestValidatorComponentBase(T target, bool hasAsyncValidation)
            : base(target, hasAsyncValidation)
        {
        }

        #endregion

        #region Properties

        public Func<string, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ValidationResult>>? GetErrorsAsyncDelegate { get; set; }

        #endregion

        #region Methods

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            GetErrorsAsyncDelegate?.Invoke(memberName, cancellationToken, metadata) ?? default;

        #endregion
    }
}