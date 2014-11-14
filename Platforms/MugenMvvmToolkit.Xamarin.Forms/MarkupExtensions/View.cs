using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using Xamarin.Forms;

namespace MugenMvvmToolkit.MarkupExtensions
{
    public static class View
    {
        #region Attached properties

        public static readonly BindableProperty BindProperty = BindableProperty.CreateAttached("Bind", typeof(string),
            typeof(View), null, propertyChanged: OnBindPropertyChanged);

        #endregion

        #region Methods

        private static void OnBindPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var bindings = (string)newValue;
            if (string.IsNullOrWhiteSpace(bindings))
                return;
            IList<IDataBinding> list = BindingServiceProvider
                .BindingProvider
                .CreateBindingsFromString(bindable, bindings);
            if (!ServiceProvider.DesignTimeManager.IsDesignMode)
                return;
            foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
                throw binding.Exception;
        }

        #endregion
    }
}