#region Copyright
// ****************************************************************************
// <copyright file="CollectionViewSourceBase.cs">
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public abstract class CollectionViewSourceBase : UICollectionViewSource
    {
        #region Fields

        private static Func<UICollectionView, IDataContext, CollectionViewSourceBase> _factory;

        private readonly UICollectionView _collectionView;
        private readonly IBindingMemberInfo _itemTemplateMember;
        private readonly HashSet<object> _selectedItems;

        private UICollectionViewCell _lastCreatedCell;
        private NSIndexPath _lastCreatedCellPath;
        private object _selectedItem;
        private bool _ignoreSelectedItems;

        #endregion

        #region Constructors

        static CollectionViewSourceBase()
        {
            _factory = (o, context) => new ItemsSourceCollectionViewSource(o);
        }

        protected CollectionViewSourceBase([NotNull] UICollectionView collectionView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(collectionView, "collectionView");
            _selectedItems = new HashSet<object>();
            _collectionView = collectionView;
            _itemTemplateMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(collectionView.GetType(), itemTemplate, false, false);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the factory that allows to create items source adapter.
        /// </summary>
        [NotNull]
        public static Func<UICollectionView, IDataContext, CollectionViewSourceBase> Factory
        {
            get { return _factory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _factory = value;
            }
        }

        public bool UseAnimations
        {
            get
            {
                return PlatformDataBindingModule
                    .CollectionViewUseAnimationsMember
                    .GetValue(_collectionView, null)
                    .GetValueOrDefault(true);
            }
            set { PlatformDataBindingModule.CollectionViewUseAnimationsMember.SetValue(_collectionView, value); }
        }

        public UICollectionViewScrollPosition ScrollPosition
        {
            get
            {
                return PlatformDataBindingModule
                    .CollectionViewScrollPositionMember
                    .GetValue(CollectionView, null).GetValueOrDefault(UICollectionViewScrollPosition.Top);
            }
            set { PlatformDataBindingModule.CollectionViewScrollPositionMember.SetValue(CollectionView, value); }
        }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set { SetSelectedItem(value, true); }
        }

        protected UICollectionView CollectionView
        {
            get { return _collectionView; }
        }

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            _collectionView.ReloadData();
        }

        public virtual void ItemSelected(object item)
        {
            if (_ignoreSelectedItems || (!_collectionView.AllowsSelection && !_collectionView.AllowsMultipleSelection))
                return;
            if (!_collectionView.AllowsMultipleSelection)
                _selectedItems.Clear();
            if (item != null && _selectedItems.Add(item))
                SetSelectedItem(item, false);
        }

        public virtual void ItemDeselected(object item)
        {
            if (_ignoreSelectedItems || (!_collectionView.AllowsSelection && !_collectionView.AllowsMultipleSelection))
                return;

            if (item != null && _selectedItems.Remove(item))
            {
                if (_selectedItems.Count == 0)
                    SetSelectedItem(null, false);
                else if (Equals(SelectedItem, item))
                    SetSelectedItem(_selectedItems.FirstOrDefault(), false);
            }
        }

        protected abstract object GetItemAt(NSIndexPath indexPath);

        protected abstract void SetSelectedCellByItem(object selectedItem);

        protected void ClearSelection()
        {
            var indexPaths = _collectionView.GetIndexPathsForSelectedItems();
            if (indexPaths != null)
            {
                foreach (NSIndexPath indexPath in indexPaths)
                    _collectionView.DeselectItem(indexPath, UseAnimations);
            }
            _selectedItems.Clear();
            SetSelectedItem(null, false);
        }

        internal UICollectionViewCell CellForItem(UICollectionView view, NSIndexPath path)
        {
            if (path == null)
                return null;
            UICollectionViewCell cell = view.CellForItem(path);
            if (cell != null)
                return cell;
            if (_lastCreatedCellPath != null && path.Equals(_lastCreatedCellPath))
                return _lastCreatedCell;
            return null;
        }

        private void SetSelectedItem(object value, bool sourceUpdate)
        {
            if (Equals(_selectedItem, value))
                return;
            _selectedItem = value;
            if (sourceUpdate)
                SetSelectedCellByItem(value);
            PlatformDataBindingModule.CollectionViewSelectedItemChangedEvent.Raise(_collectionView, EventArgs.Empty);
        }

        private void SetCellContext(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath, object item)
        {
            _ignoreSelectedItems = true;
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = item;
            _ignoreSelectedItems = false;

            if (collectionView.AllowsMultipleSelection)
            {
                if (cell.Selected)
                {
                    collectionView.SelectItem(indexPath, UseAnimations, UICollectionViewScrollPosition.None);
                    ItemSelected(item);
                }
                else
                {
                    collectionView.DeselectItem(indexPath, false);
                    ItemDeselected(item);
                }
            }
            else
                cell.Selected = collectionView.AllowsSelection && _selectedItems.Contains(item ?? cell);
        }

        #endregion

        #region Overrides of UICollectionViewSource

        public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell,
            NSIndexPath indexPath)
        {
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = null;
            cell.Selected = false;
        }

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var selector = _itemTemplateMember.TryGetValue<ICollectionCellTemplateSelector>(collectionView);
            if (selector == null)
                throw new NotSupportedException(
                    "The ItemTemplate is null to create UICollectionViewCell use the ItemTemplate with ICollectionCellTemplateSelector value.");
            object item = GetItemAt(indexPath);
            NSString identifier = selector.GetIdentifier(item, collectionView);
            var cell = (UICollectionViewCell)collectionView.DequeueReusableCell(identifier, indexPath);
            if (!BindingServiceProvider.ContextManager.HasBindingContext(cell))
                selector.InitializeTemplate(collectionView, cell);

            SetCellContext(collectionView, cell, indexPath, item);
            _lastCreatedCell = cell;
            _lastCreatedCellPath = indexPath;
            return cell;
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            ItemDeselected(GetItemAt(indexPath));
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            ItemSelected(GetItemAt(indexPath));
        }

        public override void ItemHighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UICollectionViewCell cell = CellForItem(collectionView, indexPath);
            if (cell != null)
                PlatformDataBindingModule.CollectionViewCellHighlightedMember.Raise(cell, EventArgs.Empty);
        }

        public override void ItemUnhighlighted(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UICollectionViewCell cell = CellForItem(collectionView, indexPath);
            if (cell != null)
                PlatformDataBindingModule.CollectionViewCellHighlightedMember.Raise(cell, EventArgs.Empty);
        }

        public override int NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        public override bool ShouldDeselectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldDeselectMember
                .TryGetValue(CellForItem(collectionView, indexPath), true);
        }

        public override bool ShouldHighlightItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldHighlightMember
                .TryGetValue(CellForItem(collectionView, indexPath), true);
        }

        public override bool ShouldSelectItem(UICollectionView collectionView, NSIndexPath indexPath)
        {
            return PlatformDataBindingModule
                .CollectionViewCellShouldSelectMember
                .TryGetValue(CellForItem(collectionView, indexPath), true);
        }

        #endregion
    }
}