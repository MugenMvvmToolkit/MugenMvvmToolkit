using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorComponent : IComponent<IValidator>
    {
        bool HasErrors { get; }

        IReadOnlyList<object> GetErrors(string? memberName, IReadOnlyMetadataContext? metadata);

        IReadOnlyDictionary<string, IReadOnlyList<object>> GetErrors(IReadOnlyMetadataContext? metadata);

        Task ValidateAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        void ClearErrors(string? memberName, IReadOnlyMetadataContext? metadata);
    }
}