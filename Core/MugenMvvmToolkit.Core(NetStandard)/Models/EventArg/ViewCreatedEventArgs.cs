using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewCreatedEventArgs : ViewInitializedEventArgs
    {
        #region Constructors

        public ViewCreatedEventArgs([NotNull] object view, [CanBeNull] IViewModel viewModel, [NotNull] IViewMappingItem viewMappingItem, [CanBeNull] IDataContext context)
            : base(view, viewModel, context)
        {
            Should.NotBeNull(viewMappingItem, nameof(viewMappingItem));
            ViewMappingItem = viewMappingItem;
        }

        protected ViewCreatedEventArgs()
        {
        }

        #endregion

        #region Properties

        [NotNull]
        public IViewMappingItem ViewMappingItem { get; protected set; }

        #endregion
    }
}