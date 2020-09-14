using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Validation;

namespace MugenMvvm.UnitTests.Validation.Internal
{
    public class TestValidationRule : IValidationRule
    {
        #region Properties

        public bool IsAsync { get; set; }

        public Func<object, string, IDictionary<string, object?>, CancellationToken, IReadOnlyMetadataContext?, Task?>? ValidateAsync { get; set; }

        #endregion

        #region Implementation of interfaces

        Task? IValidationRule.ValidateAsync(object target, string memberName, IDictionary<string, object?> errors, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
            => ValidateAsync?.Invoke(target, memberName, errors, cancellationToken, metadata);

        #endregion
    }
}