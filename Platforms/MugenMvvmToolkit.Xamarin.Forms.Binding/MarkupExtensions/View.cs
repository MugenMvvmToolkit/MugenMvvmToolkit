#region Copyright

// ****************************************************************************
// <copyright file="View.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Xamarin.Forms.Binding;
using Xamarin.Forms;

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Xamarin.Forms.MarkupExtensions
{
    public static class View
    {
        #region Attached properties

        public static readonly BindableProperty BindProperty = BindableProperty.CreateAttached("Bind", typeof(string),
            typeof(View), null, propertyChanged: OnBindPropertyChanged);

        #endregion

        #region Methods

        public static string GetBind(BindableObject view)
        {
            return (string)view.GetValue(BindProperty);
        }

        public static void SetBind(BindableObject view, string value)
        {
            view.SetValue(BindProperty, value);
        }

        private static void OnBindPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var bindings = (string)newValue;
            if (string.IsNullOrWhiteSpace(bindings))
                return;
            if (XamarinFormsToolkitExtensions.IsDesignMode)
            {
                XamarinFormsDataBindingExtensions.InitializeFromDesignContext();
                IList<IDataBinding> list = BindingServiceProvider.BindingProvider.CreateBindingsFromStringWithBindings(bindable, bindings);
                foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
                    throw binding.Exception;
            }
            else
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(bindable, bindings);
        }

        #endregion
    }
}
