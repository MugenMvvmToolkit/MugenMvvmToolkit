#region Copyright

// ****************************************************************************
// <copyright file="MvvmPickerViewModel.cs">
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
using System.Collections;
using System.Collections.Specialized;
using JetBrains.Annotations;
using UIKit;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class MvvmPickerViewModel : UIPickerViewModel
    {
        #region Fields

        private readonly UIPickerView _pickerView;
        private readonly NotifyCollectionChangedEventHandler _weakHandler;
        private IEnumerable _itemsSource;
        private object _selectedItem;
        private string _displayMemberPath;

        #endregion

        #region Constructors

        protected internal MvvmPickerViewModel(IntPtr handle)
            : base(handle)
        {
        }

        public MvvmPickerViewModel([NotNull] UIPickerView pickerView)
        {
            Should.NotBeNull(pickerView, "pickerView");
            _pickerView = pickerView;
            _weakHandler = ReflectionExtensions.MakeWeakCollectionChangedHandler(this,
                (adapter, o, arg3) => adapter.OnCollectionChanged(o, arg3));
            EmptyTitle = "-";
        }

        #endregion

        #region Properties

        public string EmptyTitle { get; set; }

        public string DisplayMemberPath
        {
            get { return _displayMemberPath; }
            set
            {
                _displayMemberPath = value;
                ReloadData();
            }
        }

        public virtual object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                SetSelectedItem(value);
            }
        }

        public virtual IEnumerable ItemsSource
        {
            get { return _itemsSource; }
            set { SetItemsSource(value); }
        }

        #endregion

        #region Events

        public event EventHandler SelectedItemChanged;

        #endregion

        #region Methods

        protected virtual void SetItemsSource(IEnumerable value)
        {
            if (ReferenceEquals(value, _itemsSource))
                return;
            if (_weakHandler == null)
                _itemsSource = value;
            else
            {
                IEnumerable oldValue = _itemsSource;
                var notifyCollectionChanged = oldValue as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged -= _weakHandler;
                _itemsSource = value;
                notifyCollectionChanged = value as INotifyCollectionChanged;
                if (notifyCollectionChanged != null)
                    notifyCollectionChanged.CollectionChanged += _weakHandler;
            }
            ReloadData();
            SetSelectedItem(SelectedItem);
        }

        protected virtual void OnCollectionChanged(object o, NotifyCollectionChangedEventArgs args)
        {
            ReloadData();
        }

        protected virtual void ReloadData()
        {
            _pickerView.ReloadComponent(0);
        }

        protected virtual void SetSelectedItem(object item)
        {
            if (_itemsSource == null)
                return;

            int position = _itemsSource.IndexOf(item);
            if (position < 0)
                return;

            bool animated = !_pickerView.Hidden;
            _pickerView.Select(position, 0, animated);
        }

        #endregion

        #region Overrides of UIPickerViewModel

        public override nint GetComponentCount(UIPickerView picker)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView picker, nint component)
        {
            if (_itemsSource == null)
                return 0;
            return _itemsSource.Count();
        }

        public override void Selected(UIPickerView picker, nint row, nint component)
        {
            _selectedItem = _itemsSource == null ? null : _itemsSource.ElementAtIndex((int) row);

            EventHandler handler = SelectedItemChanged;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        public override string GetTitle(UIPickerView picker, nint row, nint component)
        {
            if (_itemsSource == null)
                return EmptyTitle;
            object item = _itemsSource.ElementAtIndex((int) row);
            if (item == null)
                return EmptyTitle;
            if (!string.IsNullOrEmpty(DisplayMemberPath))
                item = BindingExtensions.GetValueFromPath(item, DisplayMemberPath);
            if (item == null)
                return EmptyTitle;
            return item.ToString();
        }

        #endregion
    }
}