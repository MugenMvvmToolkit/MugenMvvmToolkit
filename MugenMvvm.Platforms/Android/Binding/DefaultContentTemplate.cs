using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Binding
{
    public sealed class DefaultContentTemplate : IContentTemplateSelector
    {
        #region Implementation of interfaces

        public object? SelectTemplate(object container, object? item)
        {
            if (item is IViewModelBase viewModel && container is Object c)
                return viewModel.GetOrCreateView(c, 0).Target;
            ExceptionManager.ThrowNotSupported(nameof(item));
            return null;
        }

        #endregion
    }
}