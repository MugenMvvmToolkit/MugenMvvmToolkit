using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Ios.Interfaces;

namespace MugenMvvm.Ios.Bindings
{
    public sealed class DefaultContentTemplateSelector : IContentTemplateSelector
    {
        public static readonly DefaultContentTemplateSelector Instance = new();

        private DefaultContentTemplateSelector()
        {
        }

        public bool TrySelectTemplate(object container, object? item, out object? template)
        {
            if (item is IViewModelBase viewModel)
            {
                template = viewModel.GetOrCreateView().Target;
                return true;
            }

            template = null;
            return false;
        }
    }
}