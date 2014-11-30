#region Copyright
// ****************************************************************************
// <copyright file="TableViewSourceBase.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public abstract class TableViewSourceBase : UITableViewSource
    {
        #region Nested types

        private struct IdentifierKey : IEquatable<IdentifierKey>
        {
            #region Equality members

            public bool Equals(IdentifierKey other)
            {
                return ItemType == other.ItemType && string.Equals(Id, other.Id, StringComparison.Ordinal);
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

            public readonly string Id;
            public readonly Type ItemType;

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

        private const string InitializedPath = "@!~init";
        private static Func<UITableView, IDataContext, TableViewSourceBase> _factory;
        private static readonly ConcurrentDictionary<IdentifierKey, NSString> TypeToIdentifier;

        private readonly IBindingMemberInfo _itemTemplateMember;
        private readonly UITableView _tableView;
        private readonly HashSet<object> _selectedItems;

        private UITableViewCell _lastCreatedCell;
        private NSIndexPath _lastCreatedCellPath;
        private object _selectedItem;
        private bool _ignoreSelectedItems;

        #endregion

        #region Constructors

        static TableViewSourceBase()
        {
            TypeToIdentifier = new ConcurrentDictionary<IdentifierKey, NSString>();
            _factory = (o, context) => new ItemsSourceTableViewSource(o);
        }

        protected TableViewSourceBase([NotNull] UITableView tableView,
            string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(tableView, "tableView");
            _selectedItems = new HashSet<object>();
            _tableView = tableView;
            _itemTemplateMember = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(tableView.GetType(), itemTemplate, false, false);
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
                Should.PropertyBeNotNull(value);
                _factory = value;
            }
        }

        public bool UseAnimations
        {
            get
            {
                return PlatformDataBindingModule
                    .TableViewUseAnimationsMember
                    .GetValue(_tableView, null)
                    .GetValueOrDefault(true);
            }
            set { PlatformDataBindingModule.TableViewUseAnimationsMember.SetValue(_tableView, value); }
        }

        public UITableViewRowAnimation AddAnimation
        {
            get
            {
                return PlatformDataBindingModule
                    .TableViewAddAnimationMember
                    .GetValue(TableView, null).GetValueOrDefault(UITableViewRowAnimation.Automatic);
            }
            set { PlatformDataBindingModule.TableViewAddAnimationMember.SetValue(TableView, value); }
        }

        public UITableViewRowAnimation RemoveAnimation
        {
            get
            {
                return PlatformDataBindingModule
                    .TableViewRemoveAnimationMember
                    .GetValue(TableView, null).GetValueOrDefault(UITableViewRowAnimation.Automatic);
            }
            set { PlatformDataBindingModule.TableViewRemoveAnimationMember.SetValue(TableView, value); }
        }

        public UITableViewRowAnimation ReplaceAnimation
        {
            get
            {
                return PlatformDataBindingModule
                    .TableViewReplaceAnimationMember
                    .GetValue(TableView, null).GetValueOrDefault(UITableViewRowAnimation.Automatic);
            }
            set { PlatformDataBindingModule.TableViewReplaceAnimationMember.SetValue(TableView, value); }
        }

        public UITableViewScrollPosition ScrollPosition
        {
            get
            {
                return PlatformDataBindingModule
                    .TableViewScrollPositionMember
                    .GetValue(TableView, null).GetValueOrDefault(UITableViewScrollPosition.Middle);
            }
            set { PlatformDataBindingModule.TableViewScrollPositionMember.SetValue(TableView, value); }
        }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set { SetSelectedItem(value, true); }
        }

        protected UITableView TableView
        {
            get { return _tableView; }
        }

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            _tableView.ReloadData();
        }

        public virtual void ItemSelected(object item)
        {
            if (_ignoreSelectedItems || (!_tableView.AllowsSelection && !_tableView.AllowsMultipleSelection))
                return;
            if (!_tableView.AllowsMultipleSelection)
                _selectedItems.Clear();
            if (item != null && _selectedItems.Add(item))
                SetSelectedItem(item, false);
        }

        public virtual void ItemDeselected(object item)
        {
            if (_ignoreSelectedItems || (!_tableView.AllowsSelection && !_tableView.AllowsMultipleSelection))
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
            var rows = _tableView.IndexPathsForSelectedRows;
            if (rows != null)
            {
                foreach (NSIndexPath indexPath in rows)
                {
                    TableView.DeselectRow(indexPath, UseAnimations);
                    RowDeselected(_tableView, indexPath);
                }
            }
            _selectedItems.Clear();
            SetSelectedItem(null, false);
        }

        internal UITableViewCell CellAt(UITableView view, NSIndexPath path)
        {
            if (path == null)
                return null;
            UITableViewCell cell = view.CellAt(path);
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
            PlatformDataBindingModule.TableViewSelectedItemChangedEvent.Raise(_tableView, EventArgs.Empty);
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
                .TryGetValue(cell, false);
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
            cell.Selected = false;
        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
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

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            object item = GetItemAt(indexPath);
            _ignoreSelectedItems = true;
            BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = item;
            var bindable = cell as UITableViewCellBindable;
            if (bindable == null)
            {
                if (!ServiceProvider.AttachedValueProvider.Contains(cell, InitializedPath))
                {
                    ServiceProvider.AttachedValueProvider.SetValue(cell, InitializedPath, null);
                    SetCellBinding(tableView, cell);
                }
            }
            else
            {
                if (!bindable.Initialized)
                {
                    bindable.Initialized = true;
                    SetCellBinding(tableView, cell);
                }
            }
            _ignoreSelectedItems = false;

            if (tableView.AllowsMultipleSelection)
            {
                if (cell.Selected)
                {
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                    ItemSelected(item);
                }
                else
                {
                    tableView.DeselectRow(indexPath, false);
                    ItemDeselected(item);
                }
            }
            else
                cell.Selected = tableView.AllowsSelection && _selectedItems.Contains(item ?? cell);
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = CellAt(tableView, indexPath);
            return PlatformDataBindingModule
                .TableViewCellEditingStyleMember
                .TryGetValue(cell, UITableViewCellEditingStyle.None);
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
                    var cellStyle = PlatformDataBindingModule.TableViewDefaultCellStyleMember.TryGetValue(tableView, UITableViewCellStyle.Default);
                    cell = new UITableViewCellBindable(cellStyle, cellIdentifier);
                }
                BindingServiceProvider.ContextManager.GetBindingContext(cell).Value = null;
            }
            _lastCreatedCell = cell;
            _lastCreatedCellPath = indexPath;
            return cell;
        }

        public override int NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override bool ShouldHighlightRow(UITableView tableView, NSIndexPath rowIndexPath)
        {
            return PlatformDataBindingModule
                .TableViewCellShouldHighlightMember
                .TryGetValue(CellAt(tableView, rowIndexPath), true);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            ItemDeselected(GetItemAt(indexPath));
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            ItemSelected(GetItemAt(indexPath));
        }

        #endregion
    }
}