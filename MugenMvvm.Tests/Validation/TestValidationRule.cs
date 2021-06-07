using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Validation;

namespace MugenMvvm.Tests.Validation
{
    public class TestValidationRule : IValidationRule
    {
        public Func<object, string?, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>>>? ValidateAsync { get; set; }

        public bool IsAsync { get; set; }

        ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> IValidationRule.ValidateAsync(object target, string? member, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata) => ValidateAsync?.Invoke(target, member, cancellationToken, metadata) ?? default;
    }
}