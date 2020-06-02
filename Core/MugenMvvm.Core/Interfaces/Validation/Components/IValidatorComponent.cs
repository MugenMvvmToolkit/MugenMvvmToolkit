using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorComponent : IComponent<IValidator>
    {
        bool HasErrors { get; }

        ItemOrList<object, IReadOnlyList<object>> TryGetErrors(string? memberName, IReadOnlyMetadataContext? metadata);

        IReadOnlyDictionary<string, ItemOrList<object, IReadOnlyList<object>>>? TryGetErrors(IReadOnlyMetadataContext? metadata);

        Task? TryValidateAsync(string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        void ClearErrors(string? memberName, IReadOnlyMetadataContext? metadata);
    }
}