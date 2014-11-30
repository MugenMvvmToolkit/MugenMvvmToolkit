#region Copyright
// ****************************************************************************
// <copyright file="WorkspaceViewModel.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System.Threading.Tasks;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models.Messages;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the base class for the view-model, which are displayed in the UI.
    /// </summary>
    [BaseViewModel(Priority = 7)]
    public abstract class WorkspaceViewModel : WorkspaceViewModel<object>
    {
    }

    /// <summary>
    ///     Represents the base class for the view-model, which are displayed in the UI.
    /// </summary>
    [BaseViewModel(Priority = 7)]
    public abstract class WorkspaceViewModel<TView> : CloseableViewModel, IWorkspaceViewModel, INavigableViewModel,
        IViewAwareViewModel<TView>
        where TView : class
    {
        #region Fields

        private string _displayName;
        private bool _isSelected;
        private TView _view;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WorkspaceViewModel{TView}" /> class.
        /// </summary>
        protected WorkspaceViewModel()
        {
            _isSelected = false;
        }

        #endregion

        #region Implementation of interfaces

        /// <summary>
        ///     Called when a view-model becomes the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        void INavigableViewModel.OnNavigatedTo(INavigationContext context)
        {
            OnNavigatedTo(context);
            //To invalidate command state.
            Publish(StateChangedMessage.Empty);
        }

        /// <summary>
        ///     Called when a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        void INavigableViewModel.OnNavigatedFrom(INavigationContext context)
        {
            OnNavigatedFrom(context);
        }

        /// <summary>
        ///     Called just before a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        Task<bool> INavigableViewModel.OnNavigatingFrom(INavigationContext context)
        {
            return OnNavigatingFrom(context);
        }

        /// <summary>
        ///     Gets or sets the <see cref="IView" />.
        /// </summary>
        public TView View
        {
            get { return _view; }
            set
            {
                if (Equals(value, _view)) return;
                TView oldValue = _view;
                _view = value;
                OnViewChanged(oldValue, value);
                OnPropertyChanged("View");
            }
        }

        /// <summary>
        ///     Gets or sets the display name for the current model.
        /// </summary>
        public virtual string DisplayName
        {
            get { return _displayName; }
            set
            {
                if (value == _displayName) return;
                _displayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        /// <summary>
        ///     Gets or sets the property that indicates that current model is selected.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value.Equals(_isSelected)) return;
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Called just before a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        protected virtual Task<bool> OnNavigatingFrom(INavigationContext context)
        {
            return Empty.TrueTask;
        }

        /// <summary>
        ///     Called when a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        protected virtual void OnNavigatedFrom(INavigationContext context)
        {
        }

        /// <summary>
        ///     Called when a view-model becomes the active view-model in a frame.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        protected virtual void OnNavigatedTo(INavigationContext context)
        {
        }

        /// <summary>
        ///     Occurs when an <see cref="IView" /> for the current <see cref="WorkspaceViewModel{TView}" /> changed.
        /// </summary>
        /// <param name="oldView">
        ///     The old value of <see cref="IView" />.
        /// </param>
        /// <param name="newView">
        ///     The new value of <see cref="IView" />.
        /// </param>
        protected virtual void OnViewChanged(TView oldView, TView newView)
        {
        }

        #endregion
    }
}