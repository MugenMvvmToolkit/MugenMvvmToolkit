using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidationHandlerComponent : IComponent<IValidator>
    {
        Task TryValidateAsync(IValidator validator, string? member, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        Task WaitAsync(IValidator validator, string? member, IReadOnlyMetadataContext? metadata);
    }
}