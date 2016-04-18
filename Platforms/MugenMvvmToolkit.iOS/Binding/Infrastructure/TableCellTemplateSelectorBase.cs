#region Copyright

// ****************************************************************************
// <copyright file="TableCellTemplateSelectorBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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

using Foundation;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public abstract class TableCellTemplateSelectorBase<TSource, TTemplate> : ITableCellTemplateSelectorSupportDequeueReusableCell
        where TTemplate : UITableViewCell
    {
        #region Properties

        protected virtual bool SupportInitialize => true;

        #endregion

        #region Methods

        protected abstract NSString GetIdentifier(TSource item, UITableView container);

        protected abstract TTemplate SelectTemplate(UITableView container, NSString identifier);

        protected abstract void Initialize(TTemplate template, BindingSet<TTemplate, TSource> bindingSet);

        #endregion

        #region Implementation of ITableCellTemplateSelector

        void ITableCellTemplateSelector.Initialize(UITableView container)
        {
        }

        NSString ITableCellTemplateSelector.GetIdentifier(object item, UITableView container)
        {
            return GetIdentifier((TSource) item, container);
        }

        void ITableCellTemplateSelector.InitializeTemplate(UITableView container, UITableViewCell cell)
        {
            if (SupportInitialize)
            {
                var bindingSet = new BindingSet<TTemplate, TSource>((TTemplate) cell);
                Initialize((TTemplate) cell, bindingSet);
                bindingSet.Apply();
            }
        }

        UITableViewCell ITableCellTemplateSelectorSupportDequeueReusableCell.DequeueReusableCell(UITableView tableView, object item, NSIndexPath indexPath)
        {
            var identifier = GetIdentifier((TSource) item, tableView);
            return tableView.DequeueReusableCell(identifier) ?? SelectTemplate(tableView, identifier);
        }

        #endregion
    }
}