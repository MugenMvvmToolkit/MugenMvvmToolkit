using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Validation.Components
{
    public sealed class InlineValidationHandler : ValidationHandlerBase<object>
    {
        public InlineValidationHandler(object target) : base(target)
        {
        }

        public void SetErrors(string memberName, ItemOrIReadOnlyList<object> errors, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(memberName, nameof(memberName));
            UpdateErrors(memberName, errors.GetRawValue(), false, metadata);
        }

        protected override CancellationToken GetCancellationToken(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => cancellationToken;

        protected override ValueTask<ValidationResult> GetErrorsAsync(string memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata) => default;
    }
}