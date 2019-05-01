using System.Threading.Tasks;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorListener : IListener
    {
        void OnErrorsChanged(IValidator validator, string memberName, IReadOnlyMetadataContext metadata);

        void OnAsyncValidation(IValidator validator, string memberName, Task validationTask, IReadOnlyMetadataContext metadata);

        void OnDisposed(IValidator validator);
    }
}