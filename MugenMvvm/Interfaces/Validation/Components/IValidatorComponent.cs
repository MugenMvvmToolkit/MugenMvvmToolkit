using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorComponent : IComponent<IValidator>
    {
        bool HasErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata);

        ItemOrIReadOnlyList<object> TryGetErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata);

        IReadOnlyDictionary<string, object>? TryGetErrors(IValidator validator, IReadOnlyMetadataContext? metadata);

        Task TryValidateAsync(IValidator validator, string? memberName, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        void ClearErrors(IValidator validator, string? memberName, IReadOnlyMetadataContext? metadata);
    }
}