using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation.Components
{
    public interface IValidatorListener : IComponent<IValidator>
    {
        void OnErrorsChanged(IValidator validator, object? instance, string memberName, IReadOnlyMetadataContext? metadata);

        void OnAsyncValidation(IValidator validator, object? instance, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata);

        void OnDisposed(IValidator validator);
    }
}