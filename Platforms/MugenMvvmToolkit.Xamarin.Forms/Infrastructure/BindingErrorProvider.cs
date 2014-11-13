#region Copyright

// ****************************************************************************
// <copyright file="BindingErrorProvider.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the class that provides a user interface for indicating that a control on a form has an error associated
    ///     with it.
    /// </summary>
    public class BindingErrorProvider : BindingErrorProviderBase
    {
        #region Fields

        private const string TextColorPath = "#@!textColor";

        #endregion

        #region Constructors

        public BindingErrorProvider()
        {
            ErrorTextColor = Color.FromRgba(0.7f, 0, 0, 1);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the color of the text in case of error.
        /// </summary>
        public Color ErrorTextColor { get; set; }

        #endregion

        #region Overrides of BindingErrorProviderBase

        /// <summary>
        ///     Sets errors for binding target.
        /// </summary>
        /// <param name="target">The binding target object.</param>
        /// <param name="errors">The collection of errors</param>
        /// <param name="context">The specified context, if any.</param>
        protected override void SetErrors(object target, IList<object> errors, IDataContext context)
        {
            base.SetErrors(target, errors, context);
            var inputView = target as Entry;
            if (inputView != null)
            {
                if (errors.Count == 0)
                {
                    var originalColor = ServiceProvider
                        .AttachedValueProvider
                        .GetValue<Color?>(inputView, TextColorPath, false);
                    if (originalColor.HasValue)
                    {
                        ServiceProvider.AttachedValueProvider.Clear(inputView, TextColorPath);
                        inputView.TextColor = originalColor.Value;
                    }
                }
                else
                {
                    ServiceProvider
                        .AttachedValueProvider
                        .GetOrAdd(inputView, TextColorPath, (entry, o) => entry.TextColor, null);
                    inputView.TextColor = ErrorTextColor;
                }
            }
        }

        #endregion
    }
}