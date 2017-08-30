#region Copyright

// ****************************************************************************
// <copyright file="TableViewSourceBase.cs">
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
    public abstract class TableViewSourceBase : UITableViewSource
    {
        #region Nested types

        public class CellMediator : EventListenerList
        {
            #region Fields

            private UITableViewCell _cell;
            private NSIndexPath _path;
            private bool? _selectedBind;
            private UITableView _tableView;

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
                    _tableView.SelectRow(_path, false, UITableViewScrollPosition.None);
                else
                    _tableView.DeselectRow(_path, false);
                (_tableView.Source as TableViewSourceBase)?.UpdateSelectedItemInternal(_tableView, _path, value);
                Raise(_cell, EventArgs.Empty);
            }

            public static void SetFromCell(UITableView tableView, NSIndexPath path, bool value)
            {
                var cell = tableView.CellAt(path);
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

            public static void Attach(UITableView tableView, UITableViewCell cell, NSIndexPath path)
            {
                var mediator = GetMediator(cell, false);
                if (mediator != null)
                {
                    mediator._tableView = tableView;
                    mediator._cell = cell;
                    mediator._path = path;
                    mediator.OnAttach();
                }
            }

            public static void Deattach(UITableViewCell cell)
            {
                var mediator = GetMediator(cell, false);
                if (mediator != null)
                {
                    mediator._tableView = null;
                    mediator._cell = null;
                    mediator._path = null;
                }
            }

            public static CellMediator GetMediator(UITableViewCell cell, bool add)
            {
                if (add)
                    return ServiceProvider.AttachedValueProvider.GetOrAdd(cell, nameof(CellMediator), (viewCell, o) => new CellMediator(), null);
                return ServiceProvider.AttachedValueProvider.GetValue<CellMediator>(cell, nameof(CellMediator), false);
            }

            #endregion
        }

        #endregion

        #region Fields

        protected const int InitializedStateMask = 1;

        private readonly DataTemplateProvider<ITableCellTemplateSelector> _templateProvider;
        private readonly WeakReference _tableView;
        private readonly ReflectionExtensions.IWeakEventHandler<EventArgs> _listener;
        private object _selectedItem;

        #endregion

        #region Constructors

        protected TableViewSourceBase(IntPtr handle)
            : base(handle)
        {
        }

        protected TableViewSourceBase([NotNull] UITableView tableView, string itemTemplate = AttachedMemberConstants.ItemTemplate)
        {
            Should.NotBeNull(tableView, nameof(tableView));
            _tableView = ServiceProvider.WeakReferenceFactory(tableView);
            _templateProvider = new DataTemplateProvider<ITableCellTemplateSelector>(tableView, itemTemplate);
            var controllerView = tableView.FindParent<IViewControllerView>();
            if (controllerView != null && !(controllerView is IMvvmNavigationController))
            {
                _listener = ReflectionExtensions.CreateWeakEventHandler<TableViewSourceBase, EventArgs>(this, (adapter, o, arg3) => adapter.OnDisposeController(o, arg3));
                controllerView.Mediator.DisposeHandler += _listener.Handle;
            }

            UseAnimations = tableView
                .GetBindingMemberValue(AttachedMembers.UITableView.UseAnimations)
                .GetValueOrDefault(true);
            AddAnimation = tableView
                .GetBindingMemberValue(AttachedMembers.UITableView.AddAnimation)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            RemoveAnimation = tableView
                .GetBindingMemberValue(AttachedMembers.UITableView.RemoveAnimation)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            ReplaceAnimation = tableView
                .GetBindingMemberValue(AttachedMembers.UITableView.ReplaceAnimation)
                .GetValueOrDefault(UITableViewRowAnimation.Automatic);
            ScrollPosition = tableView
                .GetBindingMemberValue(AttachedMembers.UITableView.ScrollPosition)
                .GetValueOrDefault(UITableViewScrollPosition.Middle);
        }

        #endregion

        #region Properties

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

        public DataTemplateProvider<ITableCellTemplateSelector> DataTemplateProvider => _templateProvider;

        [CanBeNull]
        protected UITableView TableView => (UITableView)_tableView?.Target;

        #endregion

        #region Methods

        public virtual void ReloadData()
        {
            TableView?.ReloadData();
        }

        public abstract object GetItemAt(NSIndexPath indexPath);

        public abstract void SetSelectedCellByItem(UITableView tableView, object selectedItem);

        protected virtual void InitializeCell(UITableViewCell cell)
        {
        }

        protected virtual void OnDisposeController(object sender, EventArgs eventArgs)
        {
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

        private void SetSelectedItem(UITableView tableView, object value, bool sourceUpdate)
        {
            if (Equals(_selectedItem, value))
                return;
            _selectedItem = value;
            if (sourceUpdate)
                SetSelectedCellByItem(tableView, value);
            tableView.TryRaiseAttachedEvent(AttachedMembers.UITableView.SelectedItemChangedEvent);
        }

        private void UpdateSelectedItemInternal(UITableView tableView, NSIndexPath indexPath, bool selected)
        {
            if (tableView != null && indexPath != null)
                UpdateSelectedItemInternal(tableView, GetItemAt(indexPath), selected);
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
            tableView.CellAt(indexPath).TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.AccessoryButtonTappedEvent);
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle,
            NSIndexPath indexPath)
        {
            var cell = tableView.CellAt(indexPath);
            if (cell == null)
                return;
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    cell.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.DeleteClickEvent);
                    break;
                case UITableViewCellEditingStyle.Insert:
                    cell.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.InsertClickEvent);
                    break;
            }
        }

        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            string value;
            tableView.CellAt(indexPath).TryGetBindingMemberValue(AttachedMembers.UITableViewCell.TitleForDeleteConfirmation, out value);
            return value ?? "Delete";
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            object item = GetItemAt(indexPath);
            var selector = _templateProvider.TemplateSelector;
            if (selector == null)
                throw new NotSupportedException("The ItemTemplate is null to create UITableViewCell use the ItemTemplate with ITableCellTemplateSelector value.");
            var cellTemplateSelectorSupportDequeueReusableCell = selector as ITableCellTemplateSelectorSupportDequeueReusableCell;
            var cell = cellTemplateSelectorSupportDequeueReusableCell == null
                ? tableView.DequeueReusableCell(selector.GetIdentifier(item, tableView), indexPath)
                : cellTemplateSelectorSupportDequeueReusableCell.DequeueReusableCell(tableView, item, indexPath);

            cell.SetDataContext(item);
            if (!HasMask(cell, InitializedStateMask))
            {
                cell.Tag |= InitializedStateMask;
                ParentObserver.Set(cell, tableView);
                selector.InitializeTemplate(tableView, cell);
                InitializeCell(cell);
            }
            return cell;
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            CellMediator.Attach(tableView, cell, indexPath);
        }

        public override void CellDisplayingEnded(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            CellMediator.Deattach(cell);
        }

        public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCellEditingStyle? value;
            tableView.CellAt(indexPath).TryGetBindingMemberValue(AttachedMembers.UITableViewCell.EditingStyle, out value);
            return value.GetValueOrDefault(UITableViewCellEditingStyle.None);
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(tableView, indexPath, true);
            CellMediator.SetFromCell(tableView, indexPath, true);
        }

        public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
        {
            UpdateSelectedItemInternal(tableView, indexPath, false);
            CellMediator.SetFromCell(tableView, indexPath, false);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _selectedItem = null;
                var tableView = TableView;
                if (tableView.IsAlive())
                {
                    if (ReferenceEquals(tableView.Source, this))
                        tableView.Source = null;
                    var controllerView = tableView.FindParent<IViewControllerView>();
                    if (controllerView?.Mediator != null && _listener != null)
                        controllerView.Mediator.DisposeHandler -= _listener.Handle;
                }
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}
