using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Bindings
{
    public sealed class DefaultContentTemplateSelector : IContentTemplateSelector, IFragmentTemplateSelector
    {
        public bool HasFragments { get; set; }

        public object SelectTemplate(object container, object? item)
        {
            if (item is IViewModelBase viewModel && container is Object c)
                return viewModel.GetOrCreateView(c, 0).Target;
            ExceptionManager.ThrowNotSupported(nameof(item));
            return null;
        }
    }
}