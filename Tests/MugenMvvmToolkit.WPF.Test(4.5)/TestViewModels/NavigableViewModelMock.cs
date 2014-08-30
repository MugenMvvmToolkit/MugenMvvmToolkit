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

        /// <summary>
        ///     Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        public Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            if (OnNavigatingFromDelegate != null)
                return OnNavigatingFromDelegate(context);
            return Empty.TrueTask;
        }

        /// <summary>
        ///     Called when a page is no longer the active page in a frame.
        /// </summary>
        public void OnNavigatedFrom(INavigationContext context)
        {
            if (OnNavigatedFromDelegate != null)
                OnNavigatedFromDelegate(context);
        }

        /// <summary>
        ///     Called when a page becomes the active page in a frame.
        /// </summary>
        public void OnNavigatedTo(INavigationContext context)
        {
            if (OnNavigatedToDelegate != null)
                OnNavigatedToDelegate(context);
        }

        #endregion

        #region Implementation of IHasOperationResult

        /// <summary>
        ///     Gets or sets the operation result value.
        /// </summary>
        public bool? OperationResult { get; set; }

        #endregion

        #region Implementation of ICloseableViewModel

        /// <summary>
        ///     Gets or sets a command that attempts to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand { get; set; }

        /// <summary>
        ///     Tries to close view-model.
        /// </summary>
        /// <param name="parameter">The specified parameter, if any.</param>
        /// <returns>An instance of task with result.</returns>
        public Task<bool> CloseAsync(object parameter)
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

        /// <summary>
        ///     Gets or sets the property that indicates that current model is selected.
        /// </summary>
        public bool IsSelected { get; set; }

        #endregion
    }
}