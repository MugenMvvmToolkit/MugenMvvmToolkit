using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationHandlerBase : ValidationHandlerBase
    {
        public Func<IValidator, string?, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>>? Validate { get; set; }

        protected override ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) => Validate?.Invoke(validator, member, cancellationToken, metadata) ?? default;
    }
}