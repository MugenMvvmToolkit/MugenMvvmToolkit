using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorListener : IComponent<IValidator>
    {
        void OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext? metadata);

        void OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata);

        void OnDisposed(IValidator validator);
    }
}