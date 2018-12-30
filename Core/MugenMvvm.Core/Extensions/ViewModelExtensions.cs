using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Extensions
{
    public static class ViewModelExtensions
    {
        #region Methods

        public static void InvalidateCommands(this IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            viewModel.Publish(Default.EmptyPropertyChangedArgs);
        }

        #endregion
    }
}