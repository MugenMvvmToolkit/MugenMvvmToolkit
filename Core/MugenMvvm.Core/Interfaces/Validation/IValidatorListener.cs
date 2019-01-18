using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Validation
{
    public interface IValidatorListener
    {
        void OnErrorsChanged(string memberName);

        void OnAsyncValidation(string memberName, Task validationTask);
    }
}