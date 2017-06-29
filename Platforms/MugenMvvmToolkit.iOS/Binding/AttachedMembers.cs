#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembers.cs">
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
using System.Collections;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding
{
    public static class AttachedMembers
    {
        #region Nested types

        public abstract class Object : AttachedMembersBase.Object
        {
        }

        public abstract class UIView : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIView, object> Content;
            public static readonly BindingMemberDescriptor<UIKit.UIView, IContentViewManager> ContentViewManager;
            public static readonly BindingMemberDescriptor<UIKit.UIView, IDataTemplateSelector> ContentTemplateSelector;

            public static readonly BindingMemberDescriptor<UIKit.UIView, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<UIKit.UIView, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<UIKit.UIView, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UIView, ICollectionViewManager> CollectionViewManager;

            public static readonly BindingMemberDescriptor<UIKit.UIView, bool> Visible;

            #endregion

            #region Constructors

            static UIView()
            {
                Content = new BindingMemberDescriptor<UIKit.UIView, object>(AttachedMemberConstants.Content);
                ContentTemplateSelector = new BindingMemberDescriptor<UIKit.UIView, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector);
                ContentViewManager = new BindingMemberDescriptor<UIKit.UIView, IContentViewManager>(nameof(ContentViewManager));

                ItemsSource = new BindingMemberDescriptor<UIKit.UIView, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = new BindingMemberDescriptor<UIKit.UIView, IItemsSourceGenerator>(ItemsSourceGeneratorBase.MemberDescriptor);
                ItemTemplateSelector = new BindingMemberDescriptor<UIKit.UIView, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<UIKit.UIView, ICollectionViewManager>(nameof(CollectionViewManager));

                Visible = new BindingMemberDescriptor<UIKit.UIView, bool>(nameof(Visible));
            }

            #endregion
        }

        public abstract class UIControl : UIView
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIControl, IEventListener> ClickEvent;

            #endregion

            #region Constructors

            static UIControl()
            {
                ClickEvent = new BindingMemberDescriptor<UIKit.UIControl, IEventListener>("Click");
            }

            #endregion
        }

        public abstract class UIButton : UIControl
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIButton, string> Title;
            public static readonly BindingMemberDescriptor<UIKit.UIButton, UIControlState> State;

            #endregion

            #region Constructors

            static UIButton()
            {
                Title = new BindingMemberDescriptor<UIKit.UIButton, string>(nameof(Title));
                State = new BindingMemberDescriptor<UIKit.UIButton, UIControlState>(nameof(State));
            }

            #endregion
        }

        public abstract class UIDatePicker : UIControl
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIDatePicker, DateTime> Date;

            #endregion

            #region Constructors

            static UIDatePicker()
            {
                Date = new BindingMemberDescriptor<UIKit.UIDatePicker, DateTime>(nameof(Date));
            }

            #endregion
        }

        public abstract class UISwitch : UIControl
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UISwitch, bool> On;

            #endregion

            #region Constructors

            static UISwitch()
            {
                On = new BindingMemberDescriptor<UIKit.UISwitch, bool>(nameof(On));
            }

            #endregion
        }

        public abstract class UITextField : UIControl
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UITextField, IEventListener> TextChangedEvent;

            #endregion

            #region Constructors

            static UITextField()
            {
                TextChangedEvent = new BindingMemberDescriptor<UIKit.UITextField, IEventListener>("TextChanged");
            }

            #endregion
        }

        public abstract class UITextView : UIView //UIScrollView
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UITextView, IEventListener> TextChangedEvent;

            #endregion

            #region Constructors

            static UITextView()
            {
                TextChangedEvent = new BindingMemberDescriptor<UIKit.UITextView, IEventListener>("TextChanged");
            }

            #endregion
        }

        public abstract class UILabel : UIView
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UILabel, string> TextSizeToFit;

            #endregion

            #region Constructors

            static UILabel()
            {
                TextSizeToFit = new BindingMemberDescriptor<UIKit.UILabel, string>(nameof(TextSizeToFit));
            }

            #endregion
        }

        public abstract class UITableView : UIView //UIScrollView
        {
            #region Fields

            public new static readonly BindingMemberDescriptor<UIKit.UITableView, ITableCellTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, object> SelectedItem;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, IEventListener> SelectedItemChangedEvent;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, bool?> UseAnimations;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?> AddAnimation;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?> RemoveAnimation;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?> ReplaceAnimation;
            public static readonly BindingMemberDescriptor<UIKit.UITableView, UITableViewScrollPosition?> ScrollPosition;

            #endregion

            #region Constructors

            static UITableView()
            {
                ItemTemplateSelector = new BindingMemberDescriptor<UIKit.UITableView, ITableCellTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                SelectedItem = new BindingMemberDescriptor<UIKit.UITableView, object>(AttachedMemberConstants.SelectedItem);
                SelectedItemChangedEvent = new BindingMemberDescriptor<UIKit.UITableView, IEventListener>("SelectedItemChanged");
                UseAnimations = new BindingMemberDescriptor<UIKit.UITableView, bool?>(nameof(UseAnimations));
                AddAnimation = new BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?>(nameof(AddAnimation));
                RemoveAnimation = new BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?>(nameof(RemoveAnimation));
                ReplaceAnimation = new BindingMemberDescriptor<UIKit.UITableView, UITableViewRowAnimation?>(nameof(ReplaceAnimation));
                ScrollPosition = new BindingMemberDescriptor<UIKit.UITableView, UITableViewScrollPosition?>(nameof(ScrollPosition));
            }

            #endregion
        }

        public abstract class UITableViewCell : UIView
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener> AccessoryButtonTappedEvent;
            public static readonly BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener> DeleteClickEvent;
            public static readonly BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener> InsertClickEvent;
            public static readonly BindingMemberDescriptor<UIKit.UITableViewCell, string> TitleForDeleteConfirmation;
            public static readonly BindingMemberDescriptor<UIKit.UITableViewCell, UITableViewCellEditingStyle?> EditingStyle;

            #endregion

            #region Constructors

            static UITableViewCell()
            {
                AccessoryButtonTappedEvent = new BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener>("AccessoryButtonTapped");
                DeleteClickEvent = new BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener>("DeleteClick");
                InsertClickEvent = new BindingMemberDescriptor<UIKit.UITableViewCell, IEventListener>("InsertClick");
                EditingStyle = new BindingMemberDescriptor<UIKit.UITableViewCell, UITableViewCellEditingStyle?>(nameof(EditingStyle));
                TitleForDeleteConfirmation = new BindingMemberDescriptor<UIKit.UITableViewCell, string>(nameof(TitleForDeleteConfirmation));
            }

            #endregion
        }

        public abstract class UICollectionView : UIView //UIScrollView
        {
            #region Fields

            public new static readonly BindingMemberDescriptor<UIKit.UICollectionView, ICollectionCellTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UICollectionView, object> SelectedItem;
            public static readonly BindingMemberDescriptor<UIKit.UICollectionView, IEventListener> SelectedItemChangedEvent;
            public static readonly BindingMemberDescriptor<UIKit.UICollectionView, bool?> UseAnimations;
            public static readonly BindingMemberDescriptor<UIKit.UICollectionView, UICollectionViewScrollPosition?> ScrollPosition;

            #endregion

            #region Constructors

            static UICollectionView()
            {
                ItemTemplateSelector = new BindingMemberDescriptor<UIKit.UICollectionView, ICollectionCellTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                SelectedItem = new BindingMemberDescriptor<UIKit.UICollectionView, object>(AttachedMemberConstants.SelectedItem);
                SelectedItemChangedEvent = new BindingMemberDescriptor<UIKit.UICollectionView, IEventListener>("SelectedItemChanged");
                UseAnimations = new BindingMemberDescriptor<UIKit.UICollectionView, bool?>(nameof(UseAnimations));
                ScrollPosition = new BindingMemberDescriptor<UIKit.UICollectionView, UICollectionViewScrollPosition?>(nameof(ScrollPosition));
            }

            #endregion
        }

        public abstract class UIPickerView : UIView
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIPickerView, object> SelectedItem;
            public static readonly BindingMemberDescriptor<UIKit.UIPickerView, string> DisplayMemberPath;

            #endregion

            #region Constructors

            static UIPickerView()
            {
                SelectedItem = new BindingMemberDescriptor<UIKit.UIPickerView, object>(AttachedMemberConstants.SelectedItem);
                DisplayMemberPath = new BindingMemberDescriptor<UIKit.UIPickerView, string>(nameof(DisplayMemberPath));
            }

            #endregion
        }

        public abstract class UIViewController : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIViewController, IItemsSourceGenerator> ToolbarItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<UIKit.UIViewController, IDataTemplateSelector> ToolbarItemTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UIViewController, IDataTemplateSelector> ToastTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UIViewController, IEnumerable> ToolbarItemsSource;

            #endregion

            #region Constructors

            static UIViewController()
            {
                ToolbarItemsSourceGenerator = new BindingMemberDescriptor<UIKit.UIViewController, IItemsSourceGenerator>(nameof(ToolbarItemsSourceGenerator));
                ToolbarItemTemplateSelector = new BindingMemberDescriptor<UIKit.UIViewController, IDataTemplateSelector>(nameof(ToolbarItemTemplateSelector));
                ToolbarItemsSource = new BindingMemberDescriptor<UIKit.UIViewController, IEnumerable>(nameof(ToolbarItemsSource));
                ToastTemplateSelector = new BindingMemberDescriptor<UIKit.UIViewController, IDataTemplateSelector>(nameof(ToastTemplateSelector));
            }

            #endregion
        }

        public abstract class UITabBarController : UIViewController
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UITabBarController, object> SelectedItem;
            public static readonly BindingMemberDescriptor<UIKit.UITabBarController, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<UIKit.UITabBarController, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<UIKit.UITabBarController, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<UIKit.UITabBarController, ICollectionViewManager> CollectionViewManager;

            #endregion

            #region Constructors

            static UITabBarController()
            {
                ItemsSourceGenerator = new BindingMemberDescriptor<UIKit.UITabBarController, IItemsSourceGenerator>(ItemsSourceGeneratorBase.MemberDescriptor);
                SelectedItem = new BindingMemberDescriptor<UIKit.UITabBarController, object>(AttachedMemberConstants.SelectedItem);
                ItemsSource = new BindingMemberDescriptor<UIKit.UITabBarController, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemTemplateSelector = new BindingMemberDescriptor<UIKit.UITabBarController, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<UIKit.UITabBarController, ICollectionViewManager>(UIView.CollectionViewManager);
            }

            #endregion
        }

        public abstract class UISplitViewController : UIViewController
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UISplitViewController, UIKit.UIViewController> MasterView;
            public static readonly BindingMemberDescriptor<UIKit.UISplitViewController, UIKit.UIViewController> DetailView;

            #endregion

            #region Constructors

            static UISplitViewController()
            {
                MasterView = new BindingMemberDescriptor<UIKit.UISplitViewController, UIKit.UIViewController>(nameof(MasterView));
                DetailView = new BindingMemberDescriptor<UIKit.UISplitViewController, UIKit.UIViewController>(nameof(DetailView));
            }

            #endregion
        }

        public abstract class Element : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<global::MonoTouch.Dialog.Element, ICollectionViewManager> CollectionViewManager;

            #endregion

            #region Constructors

            static Element()
            {
                ItemsSource = new BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = new BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IItemsSourceGenerator>(UIView.ItemsSourceGenerator);
                ItemTemplateSelector = new BindingMemberDescriptor<global::MonoTouch.Dialog.Element, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<global::MonoTouch.Dialog.Element, ICollectionViewManager>(UIView.CollectionViewManager);
            }

            #endregion
        }

        public abstract class StringElement : Element
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::MonoTouch.Dialog.StringElement, IEventListener> TappedEvent;

            #endregion

            #region Constructors

            static StringElement()
            {
                TappedEvent = new BindingMemberDescriptor<global::MonoTouch.Dialog.StringElement, IEventListener>("Tapped");
            }

            #endregion
        }

        public abstract class UIBarButtonItem : AttachedMembers.Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<UIKit.UIBarButtonItem, IEventListener> ClickEvent;

            #endregion

            #region Constructors

            static UIBarButtonItem()
            {
                ClickEvent = new BindingMemberDescriptor<UIKit.UIBarButtonItem, IEventListener>("Clicked");
            }

            #endregion
        }

        #endregion
    }
}
