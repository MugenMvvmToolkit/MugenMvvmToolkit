using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class TestEditableViewModel : EditableViewModel<object>
    {
        public new bool HasChanges
        {
            get { return base.HasChanges; }
            set { base.HasChanges = value; }
        }
    }
}
