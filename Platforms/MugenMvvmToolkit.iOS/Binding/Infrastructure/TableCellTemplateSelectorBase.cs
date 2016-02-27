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

        protected virtual bool SupportInitialize => true;

        #endregion

        #region Methods

        protected abstract void Initialize(UITableView container);

        protected abstract NSString GetIdentifier(TSource item, UITableView container);

        protected abstract void InitializeTemplate(UITableView container, TTemplate cell, BindingSet<TTemplate, TSource> bindingSet);

        protected virtual nfloat? GetHeight(UITableView container, NSString identifier)
        {
            return null;
        }

        #endregion

        #region Implementation of ICollectionCellTemplateSelector

        void ITableCellTemplateSelector.Initialize(UITableView container)
        {
            Initialize(container);
        }

        public NSString GetIdentifier(object item, UITableView container)
        {
            return GetIdentifier((TSource)item, container);
        }

        void ITableCellTemplateSelector.InitializeTemplate(UITableView container, UITableViewCell cell)
        {
            if (!SupportInitialize)
                return;
            var bindingSet = new BindingSet<TTemplate, TSource>((TTemplate)cell);
            InitializeTemplate(container, (TTemplate)cell, bindingSet);
            bindingSet.Apply();
        }

        nfloat? ITableCellTemplateSelector.GetHeight(UITableView container, NSString identifier)
        {
            return GetHeight(container, identifier);
        }

        #endregion
    }
}
