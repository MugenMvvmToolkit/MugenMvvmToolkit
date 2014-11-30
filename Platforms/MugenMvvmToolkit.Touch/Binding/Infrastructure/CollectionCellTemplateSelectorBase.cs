#region Copyright
// ****************************************************************************
// <copyright file="CollectionCellTemplateSelectorBase.cs">
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
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     CollectionCellTemplateSelector allows the app writer to provide custom template selection logic.
    /// </summary>
    public abstract class CollectionCellTemplateSelectorBase<TSource, TTemplate> : ICollectionCellTemplateSelector
        where TTemplate : UICollectionViewCell
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
        ///     Initializes the current template selector.
        /// </summary>
        /// <param name="container"></param>
        protected abstract void Initialize(UICollectionView container);

        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        protected abstract NSString GetIdentifier(TSource item, UICollectionView container);

        /// <summary>
        ///     Initializes an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="cell">The specified cell to initialize.</param>
        /// <param name="bindingSet">The specified binding set.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        protected abstract void InitializeTemplate(UICollectionView container, TTemplate cell,
            BindingSet<TTemplate, TSource> bindingSet);

        #endregion

        #region Implementation of ICollectionCellTemplateSelector

        /// <summary>
        ///     Initializes the current template selector.
        /// </summary>
        /// <param name="container"></param>
        void ICollectionCellTemplateSelector.Initialize(UICollectionView container)
        {
            Initialize(container);
        }

        /// <summary>
        ///     Returns an app specific identifier for cell.
        /// </summary>
        /// <param name="item">The data content</param>
        /// <param name="container">The element to which the template will be applied</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        public NSString GetIdentifier(object item, UICollectionView container)
        {
            return GetIdentifier((TSource)item, container);
        }

        /// <summary>
        ///     Initializes an app specific template.
        /// </summary>
        /// <param name="container">The element to which the template will be applied</param>
        /// <param name="cell">The specified cell to initialize.</param>
        /// <returns>An app-specific template to apply, or null.</returns>
        void ICollectionCellTemplateSelector.InitializeTemplate(UICollectionView container, UICollectionViewCell cell)
        {
            if (!SupportInitialize)
                return;
            var bindingSet = new BindingSet<TTemplate, TSource>((TTemplate)cell);
            InitializeTemplate(container, (TTemplate)cell, bindingSet);
            bindingSet.Apply();
        }

        #endregion
    }
}