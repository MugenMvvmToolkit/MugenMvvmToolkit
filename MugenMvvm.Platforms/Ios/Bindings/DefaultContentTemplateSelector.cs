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

        public object? SelectTemplate(object container, object? item)
        {
            if (item is IViewModelBase viewModel)
                return viewModel.GetOrCreateView().Target;
            ExceptionManager.ThrowNotSupported(nameof(item));
            return null;
        }
    }
}