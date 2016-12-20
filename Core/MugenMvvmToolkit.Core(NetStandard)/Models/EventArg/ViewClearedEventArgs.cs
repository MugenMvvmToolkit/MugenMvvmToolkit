using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewClearedEventArgs : ViewInitializedEventArgs
    {
        #region Constructors

        public ViewClearedEventArgs([NotNull] object view, [CanBeNull] IViewModel viewModel, [CanBeNull] IDataContext context) : base(view, viewModel, context)
        {
        }

        protected ViewClearedEventArgs()
        {
        }

        #endregion
    }
}