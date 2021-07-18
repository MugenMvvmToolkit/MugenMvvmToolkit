using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Bindings
{
    public sealed class DefaultContentTemplateSelector : IContentTemplateSelector, IFragmentTemplateSelector
    {
        public bool HasFragments { get; set; }

        public bool TrySelectTemplate(object container, object? item, out object? template)
        {
            if (item == null)
            {
                template = null;
                return true;
            }

            if (item is IViewModelBase viewModel && container is Object c)
            {
                template = viewModel.GetOrCreateView(c, 0).Target;
                return true;
            }

            template = null;
            return false;
        }
    }
}