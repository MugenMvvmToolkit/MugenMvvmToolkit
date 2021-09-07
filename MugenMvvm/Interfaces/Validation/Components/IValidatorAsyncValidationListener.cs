using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorAsyncValidationListener : IComponent<IValidator>
    {
        void OnAsyncValidation(IValidator validator, string? member, Task validationTask, IReadOnlyMetadataContext? metadata);
    }
}