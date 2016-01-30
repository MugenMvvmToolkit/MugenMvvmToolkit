using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class ValidatableModel : NotifyPropertyChangedBase
    {
        public string Name { get; set; }

        public string MappingName { get; set; }
    }
}
