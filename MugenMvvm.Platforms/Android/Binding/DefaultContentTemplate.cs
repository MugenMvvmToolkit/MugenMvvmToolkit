using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Requests;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Binding
{
    public sealed class DefaultContentTemplate : IContentTemplateSelector
    {
        #region Implementation of interfaces

        public object? SelectTemplate(object container, object? item)
        {
            if (item is IViewModelBase viewModel)
                return MugenService.ViewManager.GetOrCreateView(new AndroidViewRequest(viewModel, container, 0)).Target;
            ExceptionManager.ThrowNotSupported(nameof(item));
            return null;
        }

        #endregion
    }
}