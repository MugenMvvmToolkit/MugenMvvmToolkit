#region Copyright

// ****************************************************************************
// <copyright file="ResourceDataTemplateSelectorBase.cs">
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

using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Android.Binding.Infrastructure
{
    /// <summary>
    ///     ResourceDataTemplateSelectorBase allows the app writer to provide custom template selection logic.
    /// </summary>
    public abstract class ResourceDataTemplateSelectorBase<TSource> : IResourceDataTemplateSelector
    {
        #region Implementation of IDataTemplateSelector

        /// <summary>
        ///     Returns the number of types of templates that will be selected by SelectTemplateMethod.
        /// </summary>
        public abstract int TemplateTypeCount { get; }

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply.</returns>
        int IResourceDataTemplateSelector.SelectTemplate(object item, object container)
        {
            return SelectTemplate((TSource)item, container);
        }

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        object IDataTemplateSelector.SelectTemplate(object item, object container)
        {
            return SelectTemplate((TSource)item, container);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply.</returns>
        protected abstract int SelectTemplate(TSource item, object container);

        #endregion
    }
}