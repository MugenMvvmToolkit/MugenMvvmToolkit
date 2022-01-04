using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Validation;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidationRule : TestDisposable, IValidationRule
    {
        public Func<IValidator, object, string?, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>>? ValidateAsync { get; set; }

        public bool IsAsync { get; set; }

        ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> IValidationRule.ValidateAsync(IValidator validator, object target, string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) => ValidateAsync?.Invoke(validator, target, member, cancellationToken, metadata) ?? default;
    }
}