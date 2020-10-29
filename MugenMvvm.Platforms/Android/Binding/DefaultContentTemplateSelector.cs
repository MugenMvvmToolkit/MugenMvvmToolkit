using Java.Lang;
using MugenMvvm.Android.Extensions;
using MugenMvvm.Android.Interfaces;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Android.Bindings
{
    public sealed class DefaultContentTemplateSelector : IContentTemplateSelector, IFragmentTemplateSelector
    {
        #region Fields

        public static readonly DefaultContentTemplateSelector Instance = new DefaultContentTemplateSelector();

        #endregion

        #region Constructors

        private DefaultContentTemplateSelector()
        {
        }

        #endregion

        #region Properties

        public bool HasFragments { get; set; }

        #endregion

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