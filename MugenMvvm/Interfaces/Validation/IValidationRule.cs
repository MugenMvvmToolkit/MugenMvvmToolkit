using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidationRule
    {
        bool IsAsync { get; }

        Task ValidateAsync(object target, string memberName, IDictionary<string, object?> errors, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}