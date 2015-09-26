using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Test.TestViewModels
{
    public class NavigableViewModelMock : ViewModelBase, INavigableViewModel, IHasOperationResult, ICloseableViewModel, ISelectable
    {
        #region Properties

        public Func<INavigationContext, Task<bool>> OnNavigatingFromDelegate { get; set; }
        public Func<object, Task<bool>> CloseDelegate { get; set; }
        public Action<INavigationContext> OnNavigatedFromDelegate { get; set; }
        public Action<INavigationContext> OnNavigatedToDelegate { get; set; }

        #endregion

        #region Overrides of NavigableViewModel

        public Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            if (OnNavigatingFromDelegate != null)
                return OnNavigatingFromDelegate(context);
            return Empty.TrueTask;
        }

        public void OnNavigatedFrom(INavigationContext context)
        {
            if (OnNavigatedFromDelegate != null)
                OnNavigatedFromDelegate(context);
        }

        public void OnNavigatedTo(INavigationContext context)
        {
            if (OnNavigatedToDelegate != null)
                OnNavigatedToDelegate(context);
        }

        #endregion

        #region Implementation of IHasOperationResult

        public bool? OperationResult { get; set; }

        #endregion

        #region Implementation of ICloseableViewModel

        public ICommand CloseCommand { get; set; }

        public Task<bool> CloseAsync(object parameter = null)
        {
            if (CloseDelegate == null)
                return Empty.TrueTask;
            return CloseDelegate(parameter);
        }

        public event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        public void OnClosing(ViewModelClosingEventArgs e)
        {
            var handler = Closing;
            if (handler != null) handler(this, e);
        }

        public event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;

        public void OnClosed(ViewModelClosedEventArgs e)
        {
            var handler = Closed;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Implementation of ISelectable

        public bool IsSelected { get; set; }

        #endregion
    }
}
