using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorProviderListener : IComponent<IValidatorProvider>
    {
        void OnValidatorCreated<TRequest>(IValidatorProvider provider, IValidator validator, [AllowNull]in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}