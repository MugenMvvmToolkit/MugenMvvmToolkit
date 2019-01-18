using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidator : IHasListeners<IValidatorListener>, IHasMetadata<IObservableMetadataContext>, IDisposable
    {
        bool HasErrors { get; }

        object Target { get; }

        IReadOnlyList<object> GetErrors(string? memberName);

        IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrors();

        Task ValidateAsync(string? memberName = null, CancellationToken cancellationToken = default);

        void ClearErrors(string? memberName = null);
    }
}