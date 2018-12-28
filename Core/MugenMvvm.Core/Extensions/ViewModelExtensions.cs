using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Extensions
{
    public static class ViewModelExtensions
    {
        #region Methods

        public static void InvalidateCommands(this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (viewModel is ViewModelBase vm)
                vm.InvalidateCommands();
            else
                viewModel.InternalMessenger.Publish(viewModel, Default.EmptyPropertyChangedArgs);
        }

        #endregion
    }
}