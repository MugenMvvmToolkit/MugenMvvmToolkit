using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorListener
    {
        void OnErrorsChanged(IValidator validator, string memberName);

        void OnAsyncValidation(IValidator validator, string memberName, Task validationTask);
    }
}