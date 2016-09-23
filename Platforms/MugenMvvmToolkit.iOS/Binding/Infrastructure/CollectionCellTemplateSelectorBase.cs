#region Copyright

// ****************************************************************************
// <copyright file="CollectionCellTemplateSelectorBase.cs">
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
    public abstract class CollectionCellTemplateSelectorBase<TSource, TTemplate> : ICollectionCellTemplateSelector
        where TTemplate : UICollectionViewCell
    {
        #region Fields

        private BindingSet<TTemplate, TSource> _bindingSet;

        #endregion

        #region Properties

        protected virtual bool SupportInitialize => true;

        #endregion

        #region Methods

        protected abstract void Initialize(UICollectionView container);

        protected abstract NSString GetIdentifier(TSource item, UICollectionView container);

        protected abstract void InitializeTemplate(UICollectionView container, TTemplate cell, BindingSet<TTemplate, TSource> bindingSet);

        #endregion

        #region Implementation of ICollectionCellTemplateSelector

        void ICollectionCellTemplateSelector.Initialize(UICollectionView container)
        {
            Initialize(container);
        }

        public NSString GetIdentifier(object item, UICollectionView container)
        {
            return GetIdentifier((TSource)item, container);
        }

        void ICollectionCellTemplateSelector.InitializeTemplate(UICollectionView container, UICollectionViewCell cell)
        {
            if (!SupportInitialize)
                return;
            if (_bindingSet == null)
                _bindingSet = new BindingSet<TTemplate, TSource>(null);
            try
            {
                _bindingSet.Target = (TTemplate)cell;
                InitializeTemplate(container, _bindingSet.Target, _bindingSet);
                _bindingSet.Apply();
            }
            finally
            {
                _bindingSet.Target = null;
            }
        }

        #endregion
    }
}
