#region Copyright

// ****************************************************************************
// <copyright file="TableCellTemplateSelectorBase.cs">
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

using System;
using Foundation;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     TableCellTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public abstract class TableCellTemplateSelectorBase<TSource, TTemplate> : ITableCellTemplateSelector
        where TTemplate : UITableViewCell
    {
        #region Properties

        /// <summary>
        ///     Specifies that this template supports initialize method default is <c>true</c>.
        /// </summary>
        protected virtual bool SupportInitialize
        {
            get { return true; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        protected abstract NSString GetIdentifier(TSource item, UITableView container);

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        protected abstract TTemplate SelectTemplate(UITableView container, NSString identifier);

        /// <summary>
        ///     Initializes the specified template.
        /// </summary>
        protected abstract void Initialize(TTemplate template, BindingSet<TTemplate, TSource> bindingSet);

        /// <summary>
        ///     Returns an app specific template height.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template height to apply, or null.</returns>
        protected virtual nfloat? GetHeight(UITableView container, NSString identifier)
        {
            return null;
        }

        #endregion

        #region Implementation of ITableCellTemplateSelector

        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        NSString ITableCellTemplateSelector.GetIdentifier(object item, UITableView container)
        {
            return GetIdentifier((TSource) item, container);
        }

        /// <summary>
        ///     Returns an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        UITableViewCell ITableCellTemplateSelector.SelectTemplate(UITableView container, NSString identifier)
        {
            TTemplate template = SelectTemplate(container, identifier);
            if (SupportInitialize && template != null)
            {
                var bindingSet = new BindingSet<TTemplate, TSource>(template);
                Initialize(template, bindingSet);
                bindingSet.Apply();
            }
            return template;
        }

        /// <summary>
        ///     Returns an app specific template height.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="identifier">The specified identifier.</param>
        /// <returns>An app-specific template height to apply, or null.</returns>
        nfloat? ITableCellTemplateSelector.GetHeight(UITableView container, NSString identifier)
        {
            return GetHeight(container, identifier);
        }

        #endregion
    }
}