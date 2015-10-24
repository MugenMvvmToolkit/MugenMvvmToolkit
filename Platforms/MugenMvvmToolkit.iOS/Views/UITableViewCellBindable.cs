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
using CoreGraphics;
using Foundation;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.iOS.Views
{
    [Register("UITableViewCellBindable")]
    public class UITableViewCellBindable : UITableViewCell, IHasDisplayCallback
    {
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

        public UITableViewCellBindable(CGRect frame)
            : base(frame)
        {
        }

        public UITableViewCellBindable(UITableViewCellStyle style, NSString reuseIdentifier)
            : base(style, reuseIdentifier)
        {
        }

        #endregion

        #region Properties

        internal bool? SelectedBind
        {
            get { return Selected; }
            set
            {
                if (value == null)
                    return;

                var tableViewSource = GetTableViewSource();
                if (tableViewSource != null)
                    value = tableViewSource.UpdateSelectedBindValue(this, value.Value);

                base.SetSelected(value.Value, false);
                this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Selected);
                if (tableViewSource != null)
                    tableViewSource.OnCellSelectionChanged(this, value.Value, true);
            }
        }

        #endregion

        #region Methods

        private TableViewSourceBase GetTableViewSource()
        {
            var parent = this.FindParent<UITableView>();
            if (parent == null)
                return null;
            return parent.Source as TableViewSourceBase;
        }

        #endregion

        #region Overrides of UITableViewCell

        public override void SetEditing(bool editing, bool animated)
        {
            if (editing == Editing)
                return;
            base.SetEditing(editing, animated);
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Editing);
            var tableViewSource = GetTableViewSource();
            if (tableViewSource != null)
                tableViewSource.OnCellEditingChanged(this, editing, false);
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            if (highlighted == Highlighted)
                return;
            base.SetHighlighted(highlighted, animated);
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Highlighted);
        }

        public override void SetSelected(bool selected, bool animated)
        {
            if (selected == Selected)
                return;
            base.SetSelected(selected, animated);
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Selected);
            var tableViewSource = GetTableViewSource();
            if (tableViewSource != null)
                tableViewSource.OnCellSelectionChanged(this, selected, false);
        }

        #endregion

        #region Implementation of IHasDisplayCallback

        public virtual void WillDisplay()
        {
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Selected);
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Editing);
            this.TryRaiseAttachedEvent(AttachedMembers.UITableViewCell.Highlighted);
        }

        public virtual void DisplayingEnded()
        {
        }

        #endregion
    }
}
