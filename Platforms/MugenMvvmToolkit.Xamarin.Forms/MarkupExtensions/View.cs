#region Copyright

// ****************************************************************************
// <copyright file="View.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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