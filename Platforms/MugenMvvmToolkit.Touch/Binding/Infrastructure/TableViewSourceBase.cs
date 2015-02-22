#region Copyright

// ****************************************************************************
// <copyright file="TableViewSourceBase.cs">
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
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Views;
using UIKit;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class TableViewSourceBase : UITableViewSource
    {
        #region Nested types

        [StructLayout(LayoutKind.Auto)]
        private struct IdentifierKey : IEquatable<IdentifierKey>
        {
            #region Equality members

            public bool Equals(IdentifierKey other)
            {
                return ItemType == other.ItemType && Id.Equals(other.Id, StringComparison.Ordinal);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is IdentifierKey && Equals((IdentifierKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ItemType.GetHashCode() * 397) ^ Id.GetHashCode();
                }
            }

            public static bool operator ==(IdentifierKey left, IdentifierKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(IdentifierKey left, IdentifierKey right)
            {
                return !left.Equals(right);
            }

            #endregion

            #region Fields

            private readonly string Id;
            private readonly Type ItemType;

            #endregion

            #region Constructors

            public IdentifierKey(Type itemType, string id)
            {
                Id = id ?? "";
                ItemType = itemType;
            }

            #endregion

            #region Methods

            public NSString GetIdentifier()
            {
                return new NSString("$TableViewSourceBase_" + Id + "_" + ItemType.FullName);
            }

            #endregion
        }

        #endregion

        #region Fields

        internal const int InitializingStateMask = 1;
        private const int InitializedStateMask = 2;
        private const int SelectedFromBindingStateFalseMask = 4;
        private static Func<UITableView, IDataContext, TableViewSourceBase> _factory;
        private static readonly ConcurrentDictionary<IdentifierKey, NSString> TypeToIdentifier;

        private readonly IBindingMemberInfo _itemTemplateMember;
        private readonly WeakReference _tableView;

        private NSIndexPath _lastCreatedCellPath;
        private UITableViewCell _lastCreatedCell;
        private object _selectedItem;

        #endregion

        #region Constructors

        static TableViewSourceBase()
        {
            TypeToIdentifier = new ConcurrentDictionary<IdentifierKey, NSString>();
            _factory = (o, context) => new ItemsSourceTableViewSource(o);
        }

        protected TableViewSourceBase(IntPtr handle)
            : base(handle)
        {
        }

        protected TableViewSourceBase([NotNull] UITableView tableView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(tableView, "tableView");
            _tableView = ServiceProvider.WeakReferenceFactory(tableView, true);
            _itemTemplateMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(tableView.GetType(), itemTemplate, false, false);
            var controllerView = tableView.FindParent<IViewControllerView>();
            if (controllerView != null && !(controllerView is IMvvmNavigationController))
                controllerView.Mediator.DisposeHandler += ControllerOnDispose;

            UseAnimations = PlatformDataBindingModule
                .TableViewUseAnimationsMember
                .GetValue(tableView, null)
                .GetValueOrDefault(true);
            AddAnimation = PlatformDataBindingModule
                .TableViewAddAnimationMember
                .GetValue(tableView, null)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            RemoveAnimation = PlatformDataBindingModule
                .TableViewRemoveAnimationMember
                .GetValue(tableView, null)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            ReplaceAnimation = PlatformDataBindingModule
                .TableViewReplaceAnimationMember
                .GetValue(tableView, null)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            ScrollPosition = PlatformDataBindingModule
                .TableViewScrollPositionMember
                .GetValue(tableView, null)
                .GetValueOrDefault(UITableViewScrollPosition.Middle);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the factory that allows to create items source adapter.
        /// </summary>
        [NotNull]
        public static Func<UITableView, IDataContext, TableViewSourceBase> Factory
        {
            get { return _factory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _factory = value;
            }
        }

        public bool UseAnimations { get; set; }

        public UITableViewRowAnimation AddAnimation { get; set; }

        public UITableViewRowAnimation RemoveAnimation { get; set; }

        public UITableViewRowAnimation ReplaceAnimation { get; set; }

        public UITableViewScrollPosition ScrollPosition { get; set; }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                var tableView = TableView;
                if (tableView != null)
                    SetSelectedItem(tableView, value, true);
            }
        }

        [CanBeNull]
        protected UITableView TableView
        {
            get { return (UITableView)_tableView.Target; }
        }

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            var tableView = TableView;
            if (tableView != null)
                tableView.ReloadData();
        }

        public virtual bool UpdateSelectedBindValue(UITableViewCell cell, bool selected)
        {
            var tableView = TableView;
            if (tableView == null || tableView.AllowsMultipleSelection)
                return selected;

            if (!tableView.AllowsSelection)
                return false;
            if (HasMask(cell, InitializingStateMask))
            {
                if (Equals(ViewManager.GetDataContext(cell), SelectedItem))
                    return true;
                return selected && SelectedItem == null;
            }
            return selected;
        }

        public virtual void OnCellSelectionChanged(UITableViewCell cell, bool selected, bool setFromBinding)
        {
            var tableView = TableView;
            if (!setFromBinding || tableView == null)
                return;

            UpdateSelectedItemInternal(tableView, ViewManager.GetDataContext(cell), selected);
            var path = IndexPathForCell(tableView, cell);
            if (path == null)
                return;
            if (selected)
                tableView.SelectRow(path, false, UITableViewScrollPosition.None);
            else
            {
                try
                {
                    //NOTE sometimes this code throw an exception on iOS 8, in this case we are using the WillDisplay method to deselect row.
                    tableView.DeselectRow(path, false);
                }
                catch
                {
                    cell.Tag |= SelectedFromBindingStateFalseMask;
                }
            }
        }

        public virtual void OnCellEditingChanged(UITableViewCell cell, bool editing, bool setFromBinding)
        {
            var tableView = TableView;
            if (tableView == null)
                return;
            var path = IndexPathForCell(tableView, cell);
            if (path != null)
                UpdateSelectedItemInternal(tableView, GetItemAt(path), cell.Selected);
        }

        protected abstract object GetItemAt(NSIndexPath indexPath);

        protected abstract void SetSelectedCellByItem(UITableView tableView, object selectedItem);

        protected virtual void ControllerOnDispose(object sender, EventArgs eventArgs)
        {
            ((IViewControllerView)sender).Mediator.DisposeHandler -= ControllerOnDispose;
            Dispose();
        }

        protected void ClearSelection(UITableView tableView)
        {
            var rows = tableView.IndexPathsForSelectedRows;
            if (rows != null)
            {
                foreach (NSIndexPath indexPath in rows)
                    tableView.DeselectRow(indexPath, UseAnimations);
            }
            SetSelectedItem(tableView, null, false);
        }

        internal static bool HasMask(UITableViewCell cell, int mask)
        {
            return (cell.Tag & mask) == mask;
        }

        internal UITableViewCell CellAt(UITableView view, NSIndexPath path)
        {
            if (path == null)
                return null;
            if (_lastCreatedCellPath != null && path.Equals(_lastCreatedCellPath))
                return _lastCreatedCell;
            return view.CellAt(path);
        }

        internal NSIndexPath IndexPathForCell(UITableView tableView, UITableViewCell cell)
        {
            if (ReferenceEquals(cell, _lastCreatedCell))
                return _lastCreatedCellPath;
            return tableView.IndexPathForCell(cell);
        }

        private void SetSelectedItem(UITableView tableView, object value, bool sourceUpdate)
        {
            if (Equals(_selectedItem, value))
                return;
            _selectedItem = value;
            if (sourceUpdate)
                SetSelectedCellByItem(tableView, value);
            PlatformDataBindingModule.TableViewSelectedItemChangedEvent.Raise(tableView, EventArgs.Empty);
        }

        private static NSString GetCellIdentifier(object item, string id = null)
        {
            if (TypeToIdentifier.Count > 100)
                TypeToIdentifier.Clear();
            return TypeToIdentifier.GetOrAdd(new IdentifierKey(item == null ? typeof(object) : item.GetType(), id),
                type => type.GetIdentifier());
        }

        private static void SetCellBinding(UITableView tableView, UITableViewCell cell)
        {
            Action<UITableViewCell> bindValue = PlatformDataBindingModule
                    .TableViewCellBindMember
                    .GetValue(tableView, null);
            if (bindValue != null)
                bindValue(cell);
        }

        private void UpdateSelectedItemInternal(UITableView tableView, object item, bool selected)
        {
            if (selected)
            {
                if (!tableView.AllowsMultipleSelection || SelectedItem == null)
                    SetSelectedItem(tableView, item, false);
            }
            else if (Equals(item, SelectedItem))
                SetSelectedItem(tableView, null, false);
        }

        #endregion

        #region Overrides of UITableViewSource

        public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.CellAtEx(indexPath);
            if (cell != null)
                PlatformDataBindingModule.TableViewCellAccessoryButtonTappedEvent.Raise(cell, EventArgs.Empty);
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return !PlatformDataBindingModule.TableViewReadOnlyMember.GetValue(tableView, null);
        }

        public override bool CanMoveRow(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = CellAt(tableView, indexPath);
            return PlatformDataBindingModule
                .TableViewCellMoveableMember
                .TryGetValue(cell)
                .GetValueOrDefault();
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle,
            NSIndexPath indexPath)
        {
            var cell = tableView.CellAtEx(indexPath);
            if (cell == null)
                return;
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    PlatformDataBindingModule.TableViewCellDeleteClickEvent.Raise(cell, EventArgs.Empty);
                    break;
                case UITableViewCellEditingStyle.Insert:
                    PlatformDataBindingModule.TableViewCellInsertClickEvent.Raise(cell, EventArgs.Empty);
                    break;
            }
        }

        public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = null;
            var callback = cell as IHasDisplayCallback;
            if (callback != null)
                callback.DisplayingEnded();
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var selector = _itemTemplateMember.TryGetValue<ITableCellTemplateSelector>(tableView);
            if (selector == null)
                return tableView.RowHeight;
            var identifier = selector.GetIdentifier(GetItemAt(indexPath), tableView);
            return selector.GetHeight(tableView, identifier).GetValueOrDefault(tableView.RowHeight);
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = CellAt(tableView, indexPath);
            return PlatformDataBindingModule
                .TitleForDeleteConfirmationMember
                .TryGetValue(cell, "Delete");
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = CellAt(tableView, indexPath);
            return PlatformDataBindingModule
                .TableViewCellEditingStyleMember
                .TryGetValue(cell)
                .GetValueOrDefault(UITableViewCellEditingStyle.None);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            object item = GetItemAt(indexPath);
            var selector = _itemTemplateMember.TryGetValue<ITableCellTemplateSelector>(tableView);
            NSString cellIdentifier = selector == null
                ? GetCellIdentifier(item)
                : selector.GetIdentifier(item, tableView);
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
            if (cell == null)
            {
                if (selector != null)
                    cell = selector.SelectTemplate(tableView, cellIdentifier);
                if (cell == null)
                {
                    var cellStyle = PlatformDataBindingModule.TableViewDefaultCellStyleMember.TryGetValue(tableView).GetValueOrDefault(UITableViewCellStyle.Default);
                    cell = new UITableViewCellBindable(cellStyle, cellIdentifier);
                }
            }
            _lastCreatedCell = cell;
            _lastCreatedCellPath = indexPath;

            if (Equals(item, _selectedItem) && !cell.Selected)
                tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);

            cell.Tag |= InitializingStateMask;
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = item;
            if (!HasMask(cell, InitializedStateMask))
            {
                cell.Tag |= InitializedStateMask;
                ParentObserver.GetOrAdd(cell).Parent = tableView;
                SetCellBinding(tableView, cell);
            }
            cell.Tag &= ~InitializingStateMask;
            var initializableItem = cell as IHasDisplayCallback;
            if (initializableItem != null)
                initializableItem.WillDisplay();
            return cell;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            if (HasMask(cell, SelectedFromBindingStateFalseMask))
            {
                tableView.DeselectRow(indexPath, false);
                cell.Tag &= ~SelectedFromBindingStateFalseMask;
            }
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath)
        {
            return PlatformDataBindingModule
                .TableViewCellShouldHighlightMember
                .TryGetValue(CellAt(tableView, rowIndexPath))
                .GetValueOrDefault(true);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(tableView, GetItemAt(indexPath), true);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(tableView, GetItemAt(indexPath), false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _lastCreatedCell = null;
                _lastCreatedCellPath = null;
                _selectedItem = null;

                var tableView = TableView;
                if (tableView.IsAlive())
                {
                    if (ReferenceEquals(tableView.Source, this))
                        tableView.Source = null;
                    var controllerView = tableView.FindParent<IViewControllerView>();
                    if (controllerView != null)
                        controllerView.Mediator.DisposeHandler -= ControllerOnDispose;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}