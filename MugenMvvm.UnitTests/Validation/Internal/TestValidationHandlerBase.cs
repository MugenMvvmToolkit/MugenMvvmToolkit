using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationHandlerBase<T> : ValidationHandlerBase<T> where T : class
    {
        public TestValidationHandlerBase(T target)
            : base(target)
        {
        }

        public Func<string, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ValidationResult>>? GetErrorsAsyncDelegate { get; set; }

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) =>
            GetErrorsAsyncDelegate?.Invoke(memberName, cancellationToken, metadata) ?? default;
    }
}