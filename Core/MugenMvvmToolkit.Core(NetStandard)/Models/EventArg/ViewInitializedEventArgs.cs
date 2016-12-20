using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewInitializedEventArgs : EventArgs
    {
        #region Constructors

        public ViewInitializedEventArgs([NotNull] object view, [CanBeNull] IViewModel viewModel, [CanBeNull] IDataContext context)
        {
            Should.NotBeNull(view, nameof(view));
            ViewModel = viewModel;
            Context = context ?? DataContext.Empty;
            View = view;
        }

        protected ViewInitializedEventArgs()
        {
        }

        #endregion

        #region Properties

        public IViewModel ViewModel { get; protected set; }

        [NotNull]
        public IDataContext Context { get; protected set; }

        [NotNull]
        public object View { get; protected set; }

        #endregion
    }
}