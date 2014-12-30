#region Copyright

// ****************************************************************************
// <copyright file="UITableViewCellBindable.cs">
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
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Modules;

namespace MugenMvvmToolkit.Views
{
    [Register("UITableViewCellBindable")]
    public class UITableViewCellBindable : UITableViewCell
    {
        #region Fields

        private UITableView _tableView;
        private IBindingContext _bindingContext;

        #endregion

        #region Constructors

        public UITableViewCellBindable(UITableViewCellStyle style, string reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        public UITableViewCellBindable()
        {
        }

        public UITableViewCellBindable(NSCoder coder)
            : base(coder)
        {
        }

        public UITableViewCellBindable(NSObjectFlag t)
            : base(t)
        {
        }

        public UITableViewCellBindable(IntPtr handle)
            : base(handle)
        {
        }

        public UITableViewCellBindable(RectangleF frame)
            : base(frame)
        {
        }

        public UITableViewCellBindable(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        #endregion

        #region Properties

        protected IBindingContext BindingContext
        {
            get
            {
                if (_bindingContext == null)
                    _bindingContext = BindingServiceProvider.ContextManager.GetBindingContext(this);
                return _bindingContext;
            }
        }

        internal bool Initialized { get; set; }

        internal bool SelectedBind
        {
            get { return Selected; }
            set
            {
                var oldValue = Selected;
                Selected = value;
                var tableView = _tableView;
                if (tableView == null)
                    return;
                var indexPath = Selected == oldValue ? null : tableView.IndexPathForCell(this);
                if (indexPath == null)
                    return;
                if (value)
                    tableView.SelectRow(indexPath, false, UITableViewScrollPosition.None);
                else
                    tableView.DeselectRow(indexPath, false);
            }
        }

        #endregion

        #region Methods

        private void Raise(bool oldValue, bool newValue,
            INotifiableAttachedBindingMemberInfo<UITableViewCell, bool> member)
        {
            if (oldValue != newValue)
                member.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Overrides of UITableViewCell

        public override void SetEditing(bool editing, bool animated)
        {
            bool oldValue = Editing;
            base.SetEditing(editing, animated);
            Raise(oldValue, editing, PlatformDataBindingModule.TableViewCellEditingMember);
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            bool oldValue = Highlighted;
            base.SetHighlighted(highlighted, animated);
            Raise(oldValue, highlighted, PlatformDataBindingModule.TableViewCellHighlightedMember);
        }

        public override void SetSelected(bool selected, bool animated)
        {
            bool oldValue = Selected;
            base.SetSelected(selected, animated);
            Raise(oldValue, selected, PlatformDataBindingModule.TableViewCellSelectedMember);

            if (!Initialized)
                return;
            object dataContext = BindingContext.Value;
            UITableView tableView = _tableView;
            if (dataContext == null || tableView == null)
                return;
            var tableViewSourceBase = tableView.Source as TableViewSourceBase;
            if (tableViewSourceBase == null)
                return;
            if (selected)
                tableViewSourceBase.ItemSelected(dataContext);
            else
                tableViewSourceBase.ItemDeselected(dataContext);
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();
            this.RaiseParentChanged();
            UIView view = Superview;
            while (view != null)
            {
                _tableView = view as UITableView;
                if (_tableView != null)
                    break;
                view = view.Superview;
            }
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();
            this.RaiseParentChanged();
            _tableView = null;
        }

        #endregion
    }
}