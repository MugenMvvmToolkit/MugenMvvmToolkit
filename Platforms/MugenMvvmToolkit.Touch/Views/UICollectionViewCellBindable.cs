#region Copyright

// ****************************************************************************
// <copyright file="UICollectionViewCellBindable.cs">
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
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Modules;
using UIKit;

namespace MugenMvvmToolkit.Views
{
    [Register("UICollectionViewCellBindable")]
    public class UICollectionViewCellBindable : UICollectionViewCell
    {
        #region Fields

        private UICollectionView _collectionView;
        private IBindingContext _bindingContext;

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

        protected IBindingContext BindingContext
        {
            get
            {
                if (_bindingContext == null)
                    _bindingContext = BindingServiceProvider.ContextManager.GetBindingContext(this);
                return _bindingContext;
            }
        }

        internal bool SelectedBind
        {
            get { return Selected; }
            set
            {
                var oldValue = Selected;
                Selected = value;
                var collectionView = _collectionView;
                if (collectionView == null)
                    return;
                var indexPath = Selected == oldValue ? null : collectionView.IndexPathForCell(this);
                if (indexPath == null)
                    return;
                if (value)
                    collectionView.SelectItem(indexPath, false, UICollectionViewScrollPosition.None);
                else
                    collectionView.DeselectItem(indexPath, false);
            }
        }

        #endregion

        #region Methods

        private void Raise(bool oldValue, bool newValue, INotifiableAttachedBindingMemberInfo<UICollectionViewCell, bool> member)
        {
            if (oldValue != newValue)
                member.Raise(this, EventArgs.Empty);
        }

        #endregion

        #region Overrides of UICollectionViewCell

        public override bool Highlighted
        {
            get { return base.Highlighted; }
            set
            {
                var oldValue = Highlighted;
                base.Highlighted = value;
                Raise(oldValue, value, PlatformDataBindingModule.CollectionViewCellHighlightedMember);
            }
        }

        public override bool Selected
        {
            get { return base.Selected; }
            set
            {
                var oldValue = Selected;
                base.Selected = value;
                Raise(oldValue, value, PlatformDataBindingModule.CollectionViewCellSelectedMember);

                object dataContext = BindingContext.Value;
                var collectionView = _collectionView;
                if (dataContext == null || collectionView == null)
                    return;
                var tableViewSourceBase = collectionView.Source as CollectionViewSourceBase;
                if (tableViewSourceBase == null)
                    return;
                if (value)
                    tableViewSourceBase.ItemSelected(dataContext);
                else
                    tableViewSourceBase.ItemDeselected(dataContext);
            }
        }

        public override void MovedToSuperview()
        {
            base.MovedToSuperview();
            this.RaiseParentChanged();
            UIView view = Superview;
            while (view != null)
            {
                _collectionView = view as UICollectionView;
                if (_collectionView != null)
                    break;
                view = view.Superview;
            }
        }

        public override void RemoveFromSuperview()
        {
            base.RemoveFromSuperview();
            this.RaiseParentChanged();
            _collectionView = null;
        }

        #endregion
    }
}