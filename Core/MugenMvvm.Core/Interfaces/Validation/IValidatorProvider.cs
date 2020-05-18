using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorProvider : IComponentOwner<IValidatorProvider>, IComponent<IMugenApplication>
    {
        IValidator? GetValidator<TRequest>([AllowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null);
    }
}