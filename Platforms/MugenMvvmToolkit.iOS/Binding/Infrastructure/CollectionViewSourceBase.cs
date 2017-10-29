#region Copyright

// ****************************************************************************
// <copyright file="CollectionViewSourceBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using MugenMvvmToolkit.iOS.Binding.Models;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    public abstract class CollectionViewSourceBase : UICollectionViewSource
    {
        #region Nested types

        public class CellMediator : EventListenerList
        {
            #region Fields

            private UICollectionViewCell _cell;
            private NSIndexPath _path;
            private bool? _selectedBind;
            private UICollectionView _collectionView;

            #endregion

            #region Properties

            public bool? SelectedBind
            {
                get { return _selectedBind; }
                set
                {
                    if (value == _selectedBind)
                        return;
                    _selectedBind = value;
                    if (_cell != null && value != null)
                        SetSelected(value.Value);
                }
            }

            #endregion

            #region Methods

            private void OnAttach()
            {
                if (SelectedBind == null)
                    Raise(_cell, EventArgs.Empty);
                else
                    SetSelected(SelectedBind.Value);
            }

            private void SetSelected(bool value)
            {
                _cell.Selected = value;
                if (value)
                    _collectionView.SelectItem(_path, false, UICollectionViewScrollPosition.None);
                else
                    _collectionView.DeselectItem(_path, false);
                (_collectionView.Source as CollectionViewSourceBase)?.UpdateSelectedItemInternal(_collectionView, _path, value);
                Raise(_cell, EventArgs.Empty);
            }

            public static void SetFromCell(UICollectionView collectionView, NSIndexPath path, bool value)
            {
                var cell = collectionView.CellForItem(path);
                if (cell == null)
                    return;
                var mediator = GetMediator(cell, false);
                if (mediator != null && mediator._selectedBind != value)
                {
                    mediator._selectedBind = value;
                    if (mediator._cell != null)
                        mediator.Raise(mediator._cell, EventArgs.Empty);
                }
            }

            public static void Attach(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath path)
            {
                var mediator = GetMediator(cell, false);
                if (mediator != null)
                {
                    mediator._collectionView = collectionView;
                    mediator._cell = cell;
                    mediator._path = path;
                    mediator.OnAttach();
                }
            }

            public static void Deattach(UICollectionViewCell cell)
            {
                var mediator = GetMediator(cell, false);
                if (mediator != null)
                {
                    mediator._collectionView = null;
                    mediator._cell = null;
                    mediator._path = null;
                }
            }

            public static CellMediator GetMediator(UICollectionViewCell cell, bool add)
            {
                if (add)
                    return ToolkitServiceProvider.AttachedValueProvider.GetOrAdd(cell, nameof(CellMediator), (viewCell, o) => new CellMediator(), null);
                return ToolkitServiceProvider.AttachedValueProvider.GetValue<CellMediator>(cell, nameof(CellMediator), false);
            }

            #endregion
        }

        #endregion

        #region Fields

        protected const int InitializedStateMask = 1;

        private readonly WeakReference _collectionView;
        private readonly DataTemplateProvider<ICollectionCellTemplateSelector> _itemTemplateProvider;
        private readonly ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;
        private object _selectedItem;

        #endregion

        #region Constructors

        protected CollectionViewSourceBase(IntPtr handle)
            : base(handle)
        {
        }

        protected CollectionViewSourceBase([NotNull] UICollectionView collectionView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(collectionView, nameof(collectionView));
            _collectionView = TouchToolkitExtensions.CreateWeakReference(collectionView);
            _itemTemplateProvider = new DataTemplateProvider<ICollectionCellTemplateSelector>(collectionView, itemTemplate);
            var controllerView = collectionView.FindParent<IViewControllerView>();
            if (controllerView != null && !(controllerView is IMvvmNavigationController))
            {
                _listener = ReflectionExtensions.CreateWeakEventHandler<CollectionViewSourceBase, EventArgs>(this, (adapter, o, arg3) => adapter.OnDisposeController(o, arg3));
                controllerView.Mediator.DisposeHandler += _listener.Handle;
            }

            UseAnimations = collectionView
                .GetBindingMemberValue(AttachedMembers.UICollectionView.UseAnimations)
                .GetValueOrDefault(true);
            ScrollPosition = collectionView
                .GetBindingMemberValue(AttachedMembers.UICollectionView.ScrollPosition)
                .GetValueOrDefault(UICollectionViewScrollPosition.Top);
        }

        #endregion

        #region Properties

        public bool UseAnimations { get; set; }

        public UICollectionViewScrollPosition ScrollPosition { get; set; }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                var collectionView = CollectionView;
                if (collectionView != null)
                    SetSelectedItem(collectionView, value, true);
            }
        }

        public DataTemplateProvider<ICollectionCellTemplateSelector> DataTemplateProvider => _itemTemplateProvider;

        [CanBeNull]
        protected UICollectionView CollectionView => (UICollectionView)_collectionView?.Target;

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            CollectionView?.ReloadData();
        }

        public abstract object GetItemAt(NSIndexPath indexPath);

        public abstract void SetSelectedCellByItem(UICollectionView collectionView, object selectedItem);

        protected virtual void InitializeCell(UICollectionViewCell cell)
        {
        }

        protected virtual void OnDisposeController(object sender, EventArgs eventArgs)
        {
            Dispose();
        }

        protected void ClearSelection(UICollectionView collectionView)
        {
            var indexPaths = collectionView.GetIndexPathsForSelectedItems();
            if (indexPaths != null)
            {
                foreach (NSIndexPath indexPath in indexPaths)
                    collectionView.DeselectItem(indexPath, UseAnimations);
            }
            SetSelectedItem(collectionView, null, false);
        }

        private static bool HasMask(UICollectionViewCell cell, int mask)
        {
            return (cell.Tag & mask) == mask;
        }

        private void SetSelectedItem(UICollectionView collectionView, object value, bool sourceUpdate)
        {
            if (Equals(_selectedItem, value))
                return;
            _selectedItem = value;
            if (sourceUpdate)
                SetSelectedCellByItem(collectionView, value);
            collectionView.TryRaiseAttachedEvent(AttachedMembers.UICollectionView.SelectedItemChangedEvent);
        }

        private void UpdateSelectedItemInternal(UICollectionView collectionView, NSIndexPath indexPath, bool selected)
        {
            if (collectionView != null && indexPath != null)
                UpdateSelectedItemInternal(collectionView, GetItemAt(indexPath), selected);
        }

        private void UpdateSelectedItemInternal(UICollectionView collectionView, object item, bool selected)
        {
            if (selected)
            {
                if (!collectionView.AllowsMultipleSelection || SelectedItem == null)
                    SetSelectedItem(collectionView, item, false);
            }
            else if (Equals(item, SelectedItem))
                SetSelectedItem(collectionView, null, false);
        }

        #endregion

        #region Overrides of UICollectionViewSource

        public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
        {
            var selector = _itemTemplateProvider.TemplateSelector;
            if (selector == null)
                throw new NotSupportedException("The ItemTemplate is null to create UICollectionViewCell use the ItemTemplate with ICollectionCellTemplateSelector value.");
            object item = GetItemAt(indexPath);
            UICollectionViewCell cell;
            var cellTemplateSelectorSupportDequeueReusableCell = selector as ICollectionCellTemplateSelectorSupportDequeueReusableCell;
            if (cellTemplateSelectorSupportDequeueReusableCell == null)
                cell = (UICollectionViewCell)collectionView.DequeueReusableCell(selector.GetIdentifier(item, collectionView), indexPath);
            else
                cell = cellTemplateSelectorSupportDequeueReusableCell.DequeueReusableCell(collectionView, item, indexPath);

            cell.SetDataContext(item);
            if (!HasMask(cell, InitializedStateMask))
            {
                cell.Tag |= InitializedStateMask;
                ParentObserver.Set(cell, collectionView);
                selector.InitializeTemplate(collectionView, cell);
                InitializeCell(cell);
            }
            CellMediator.Attach(collectionView, cell, indexPath);
            return cell;
        }

        public override void CellDisplayingEnded(UICollectionView collectionView, UICollectionViewCell cell, NSIndexPath indexPath)
        {
            CellMediator.Deattach(cell);
        }

        public override void ItemDeselected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(collectionView, indexPath, false);
            CellMediator.SetFromCell(collectionView, indexPath, false);
        }

        public override void ItemSelected(UICollectionView collectionView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(collectionView, indexPath, true);
            CellMediator.SetFromCell(collectionView, indexPath, true);
        }

        public override nint NumberOfSections(UICollectionView collectionView)
        {
            return 1;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _selectedItem = null;

                var collectionView = CollectionView;
                if (collectionView.IsAlive())
                {
                    if (ReferenceEquals(collectionView.Source, this))
                        collectionView.Source = null;
                    var controllerView = collectionView.FindParent<IViewControllerView>();
                    if (controllerView?.Mediator != null && _listener != null)
                        controllerView.Mediator.DisposeHandler -= _listener.Handle;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
