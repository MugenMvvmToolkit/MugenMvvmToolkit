using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorListener : IListener
    {
        void OnErrorsChanged(IValidator validator, string memberName);

        void OnAsyncValidation(IValidator validator, string memberName, Task validationTask);
    }
}