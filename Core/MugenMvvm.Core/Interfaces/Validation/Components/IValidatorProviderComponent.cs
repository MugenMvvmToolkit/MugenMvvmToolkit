using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderComponent : IComponent<IValidatorProvider>
    {
        IValidator? TryGetValidator<TRequest>([AllowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}