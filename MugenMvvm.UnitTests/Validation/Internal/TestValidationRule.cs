using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Internal;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationRule : IValidationRule
    {
        public Func<object, string, IDictionary<string, object?>, CancellationToken, IReadOnlyMetadataContext?, Task?>? ValidateAsync { get; set; }

        public bool IsAsync { get; set; }

        Task IValidationRule.ValidateAsync(object target, string memberName, IDictionary<string, object?> errors, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
            => ValidateAsync?.Invoke(target, memberName, errors, cancellationToken, metadata) ?? Default.CompletedTask;
    }
}