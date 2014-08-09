#region Copyright
// ****************************************************************************
// <copyright file="BindingResourceExtension.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System;
using System.Windows.Markup;
using MugenMvvmToolkit.Binding.Core;

namespace MugenMvvmToolkit.Binding.MarkupExtensions
{
    /// <summary>
    ///     Implements a markup extension that supports (XAML load time) resource references made from XAML.
    /// </summary>
    public sealed class BindingResourceExtension : MarkupExtension
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="BindingResourceExtension" />.
        /// </summary>
        public BindingResourceExtension()
        {
        }

#if WPF
        /// <summary>
        ///     Initializes a new instance of a class derived from <see cref="BindingResourceExtension" />.
        /// </summary>
        public BindingResourceExtension(string name)
        {
            Name = name;
        }
#endif

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the resource name.
        /// </summary>
#if WPF
        [ConstructorArgument("name")]
#endif
        public string Name { get; set; }

        #endregion

        #region Overrides of MarkupExtension

        /// <summary>
        ///     When implemented in a derived class, returns an object that is provided as the value of the target property for
        ///     this markup extension.
        /// </summary>
        /// <returns>
        ///     The object value to set on the property where the extension is applied.
        /// </returns>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return null;
            return BindingProvider
                .Instance
                .ResourceResolver
                .ResolveObject(Name, !ApplicationSettings.IsDesignMode)
                .Value;
        }

        #endregion
    }
}