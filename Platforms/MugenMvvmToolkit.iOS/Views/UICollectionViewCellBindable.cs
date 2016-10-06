#region Copyright

// ****************************************************************************
// <copyright file="UICollectionViewCellBindable.cs">
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
    [Register("UICollectionViewCellBindable")]
    public class UICollectionViewCellBindable : UICollectionViewCell, IHasDisplayCallback
    {
        #region Fields

        private WeakReference _parent;

        #endregion

        #region Constructors

        public UICollectionViewCellBindable()
        {
        }

        public UICollectionViewCellBindable(NSCoder coder)
            : base(coder)
        {
        }

        public UICollectionViewCellBindable(NSObjectFlag t)
            : base(t)
        {
        }

        public UICollectionViewCellBindable(IntPtr handle)
            : base(handle)
        {
        }

        public UICollectionViewCellBindable(CGRect frame)
            : base(frame)
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

                var tableViewSource = GetCollectionViewSource();
                if (tableViewSource != null)
                    value = tableViewSource.UpdateSelectedBindValue(this, value.Value);

                base.Selected = value.Value;
                this.TryRaiseAttachedEvent(AttachedMembers.UICollectionViewCell.Selected);
                tableViewSource?.OnCellSelectionChanged(this, value.Value, true);
            }
        }

        #endregion

        #region Methods

        private CollectionViewSourceBase GetCollectionViewSource()
        {
            if (_parent == null)
            {
                var parent = this.FindParent<UICollectionView>();
                if (parent == null)
                    return null;
                _parent = ServiceProvider.WeakReferenceFactory(parent);
            }
            return ((UICollectionView)_parent.Target)?.Source as CollectionViewSourceBase;
        }

        #endregion

        #region Overrides of UICollectionViewCell

        public override bool Highlighted
        {
            get { return base.Highlighted; }
            set
            {
                if (value == Highlighted)
                    return;
                base.Highlighted = value;
                this.TryRaiseAttachedEvent(AttachedMembers.UICollectionViewCell.Highlighted);
            }
        }

        public override bool Selected
        {
            get { return base.Selected; }
            set
            {
                if (value == Selected)
                    return;
                base.Selected = value;
                this.TryRaiseAttachedEvent(AttachedMembers.UICollectionViewCell.Selected);
                GetCollectionViewSource()?.OnCellSelectionChanged(this, value, false);
            }
        }

        #endregion

        #region Implementation of IHasDisplayCallback

        public virtual void WillDisplay()
        {
            this.TryRaiseAttachedEvent(AttachedMembers.UICollectionViewCell.Selected);
            this.TryRaiseAttachedEvent(AttachedMembers.UICollectionViewCell.Highlighted);
        }

        public virtual void DisplayingEnded()
        {
        }

        #endregion
    }
}
