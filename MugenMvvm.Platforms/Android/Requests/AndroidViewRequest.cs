using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Requests;

namespace MugenMvvm.Android.Requests
{
    public class AndroidViewRequest : ViewModelViewRequest
    {
        #region Fields

        public readonly object Container;
        public readonly int ResourceId;

        #endregion

        #region Constructors

        public AndroidViewRequest(IViewModelBase viewModel, object container, int resourceId)
            : base(viewModel, null)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(container, nameof(container));
            Container = container;
            ResourceId = resourceId;
        }

        #endregion
    }
}