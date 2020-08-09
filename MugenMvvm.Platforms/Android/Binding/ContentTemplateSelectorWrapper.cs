using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Android.Native.Views;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Binding
{
    public sealed class ContentTemplateSelectorWrapper : IContentTemplateSelector, IFragmentTemplateSelector, ITitleTemplateSelector
    {
        #region Fields

        private readonly IResourceTemplateSelector _selector;

        #endregion

        #region Constructors

        public ContentTemplateSelectorWrapper(IResourceTemplateSelector selector)
        {
            Should.NotBeNull(selector, nameof(selector));
            _selector = selector;
        }

        #endregion

        #region Properties

        public bool HasFragments
        {
            get
            {
                if (_selector is IFragmentTemplateSelector s)
                    return s.HasFragments;
                return false;
            }
        }

        #endregion

        #region Implementation of interfaces

        public object? SelectTemplate(object container, object? item)
        {
            var template = _selector.SelectTemplate(container, item);
            if (item is IViewModelBase viewModel && container is Object c)
                return viewModel.GetOrCreateView(c, template).Target;
            if (container is Object javaContainer)
                return ViewExtensions.GetView(javaContainer, template, false);
            ExceptionManager.ThrowNotSupported(nameof(container));
            return null;
        }

        public ICharSequence? GetTitle(object container, object? item)
        {
            return (_selector as ITitleTemplateSelector)?.GetTitle(container, item);
        }

        #endregion
    }
}