using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Bindings
{
    public sealed class ContentTemplateSelectorWrapper : IContentTemplateSelector, IFragmentTemplateSelector, ITitleTemplateSelector
    {
        private readonly IResourceTemplateSelector _selector;

        public ContentTemplateSelectorWrapper(IResourceTemplateSelector selector)
        {
            Should.NotBeNull(selector, nameof(selector));
            _selector = selector;
        }

        public bool HasFragments
        {
            get
            {
                if (_selector is IFragmentTemplateSelector s)
                    return s.HasFragments;
                return false;
            }
        }

        public object SelectTemplate(object container, object? item)
        {
            var template = _selector.SelectTemplate(container, item);
            if (item is IViewModelBase viewModel && container is Object c)
                return viewModel.GetOrCreateView(c, template).Target;
            if (container is Object javaContainer)
                return ViewMugenExtensions.GetView(javaContainer, template, false);
            ExceptionManager.ThrowNotSupported(nameof(container));
            return null;
        }

        public ICharSequence? GetTitle(object container, object? item) => (_selector as ITitleTemplateSelector)?.GetTitle(container, item);
    }
}