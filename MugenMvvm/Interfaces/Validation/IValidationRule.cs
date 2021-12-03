using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Validation;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidationRule : IDisposable
    {
        bool IsAsync { get; }

        ValueTask<ItemOrIReadOnlyList<ValidationErrorInfo>> ValidateAsync(object target, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}