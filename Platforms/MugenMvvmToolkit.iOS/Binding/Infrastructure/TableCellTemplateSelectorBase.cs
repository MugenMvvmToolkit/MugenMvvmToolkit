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
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public abstract class TableCellTemplateSelectorBase<TSource, TTemplate> : ITableCellTemplateSelector
        where TTemplate : UITableViewCell
    {
        #region Properties

        protected virtual bool SupportInitialize
        {
            get { return true; }
        }

        #endregion

        #region Methods

        protected abstract NSString GetIdentifier(TSource item, UITableView container);

        protected abstract TTemplate SelectTemplate(UITableView container, NSString identifier);

        protected abstract void Initialize(TTemplate template, BindingSet<TTemplate, TSource> bindingSet);

        protected virtual nfloat? GetHeight(UITableView container, NSString identifier)
        {
            return null;
        }

        #endregion

        #region Implementation of ITableCellTemplateSelector

        NSString ITableCellTemplateSelector.GetIdentifier(object item, UITableView container)
        {
            return GetIdentifier((TSource) item, container);
        }

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

        nfloat? ITableCellTemplateSelector.GetHeight(UITableView container, NSString identifier)
        {
            return GetHeight(container, identifier);
        }

        #endregion
    }
}
