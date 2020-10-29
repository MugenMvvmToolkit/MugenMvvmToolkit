using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Ios.Interfaces;

namespace MugenMvvm.Ios.Bindings
{
    public sealed class DefaultContentTemplateSelector : IContentTemplateSelector
    {
        #region Fields

        public static readonly DefaultContentTemplateSelector Instance = new DefaultContentTemplateSelector();

        #endregion

        #region Constructors

        private DefaultContentTemplateSelector()
        {
        }

        #endregion

        #region Implementation of interfaces

        public object? SelectTemplate(object container, object? item)
        {
            if (item is IViewModelBase viewModel)
                return viewModel.GetOrCreateView().Target;
            ExceptionManager.ThrowNotSupported(nameof(item));
            return null;
        }

        #endregion
    }
}