using Java.Lang;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native;
using MugenMvvm.Android.Requests;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Binding
{
    public sealed class ContentTemplateResourceSelectorWrapper : IContentTemplateSelector
    {
        #region Fields

        private readonly IDataTemplateSelector _selector;

        #endregion

        #region Constructors

        public ContentTemplateResourceSelectorWrapper(IDataTemplateSelector selector)
        {
            Should.NotBeNull(selector, nameof(selector));
            _selector = selector;
        }

        #endregion

        #region Implementation of interfaces

        public object? SelectTemplate(object container, object? item)
        {
            var template = _selector.SelectTemplate(container, item);
            if (item is IViewModelBase viewModel)
                return MugenService.ViewManager.GetOrCreateView(new AndroidViewRequest(viewModel, container, template)).Target;
            if (container is Object javaContainer)
                return MugenAndroidNativeService.GetView(javaContainer, template);
            ExceptionManager.ThrowNotSupported(nameof(container));
            return null;
        }

        #endregion
    }
}