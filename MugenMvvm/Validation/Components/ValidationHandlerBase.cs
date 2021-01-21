using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;

namespace MugenMvvm.Validation.Components
{
    public abstract class ValidationHandlerBase : IValidationHandlerComponent
    {
        public async Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            var errors = await ValidateAsync(validator, member, cancellationToken, metadata).ConfigureAwait(false);
            if (string.IsNullOrEmpty(member))
            {
                validator.ResetErrors(this, errors, metadata);
                return;
            }

            if (!errors.Contains(member))
                validator.ClearErrors(member, this, metadata);
            validator.SetErrors(this, errors, metadata);
        }

        protected abstract ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata);
    }
}