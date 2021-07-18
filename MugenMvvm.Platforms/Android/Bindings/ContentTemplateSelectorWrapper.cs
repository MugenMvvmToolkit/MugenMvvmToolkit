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

        public bool TrySelectTemplate(object container, object? item, out object? template)
        {
            var templateId = _selector.TrySelectTemplate(container, item);
            if (templateId == IResourceTemplateSelector.NoResult)
            {
                template = null;
                return false;
            }

            if (item is IViewModelBase viewModel && container is Object c)
            {
                template = viewModel.GetOrCreateView(c, templateId).Target;
                return true;
            }

            if (container is Object javaContainer)
            {
                template = ViewMugenExtensions.GetView(javaContainer, templateId, false, null!);
                return true;
            }

            template = null;
            return false;
        }

        public ICharSequence? TryGetTitle(object container, object? item) => (_selector as ITitleTemplateSelector)?.TryGetTitle(container, item);
    }
}