#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Drawing;
using System.Linq;
using Foundation;
using UIKit;
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule : DataBindingModule
    {
        #region Fields

        internal readonly static IAttachedBindingMemberInfo<object, ICollectionViewManager> CollectionViewManagerMember;

        private static readonly EventHandler<UITabBarSelectionEventArgs> SelecectedControllerChangedHandler;
        private readonly static IAttachedBindingMemberInfo<object, IContentViewManager> ContentViewManagerMember;
        private readonly static IAttachedBindingMemberInfo<object, IEnumerable> ItemsSourceMember;

        private readonly static IAttachedBindingMemberInfo<UIView, object> ContentMember;
        private readonly static IAttachedBindingMemberInfo<UIView, IDataTemplateSelector> ContentTemplateMember;
        private readonly static IAttachedBindingMemberInfo<UITabBarController, object> TabBarSelectedItemMember;

        private const string ToolbarItemTemplate = "ToolbarItemTemplate";
        private const string TabBarItemsSourceKey = "~~@!tabitems";
        private const string ToolbarItemsSourceKey = "~~@!toolbaritems";
        private const string TextChangedEvent = "~@txtchang";
        private const string ContentControllerPath = "#$!contentctr";

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            SelecectedControllerChangedHandler = TabBarOnViewControllerSelected;

            //Object
            ItemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            CollectionViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>("CollectionViewManager");
            ContentViewManagerMember = AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>("ContentViewManager");

            //UIView
            ContentMember = AttachedBindingMember.CreateAutoProperty<UIView, object>(AttachedMemberConstants.Content, ContentChanged);
            ContentTemplateMember = AttachedBindingMember.CreateAutoProperty<UIView, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplate, ContentTemplateChanged);

            //UITableView
            TableViewSelectedItemChangedEvent = AttachedBindingMember.CreateEvent<UITableView>("SelectedItemChanged");

            TableViewReadOnlyMember = AttachedBindingMember.CreateAutoProperty<UITableView, bool>("ReadOnly");
            TableViewUseAnimationsMember = AttachedBindingMember.CreateAutoProperty<UITableView, bool?>("UseAnimations");
            TableViewCellBindMember = AttachedBindingMember.CreateAutoProperty<UITableView, Action<UITableViewCell>>("CellBind");
            TableViewAddAnimationMember = AttachedBindingMember.CreateAutoProperty<UITableView, UITableViewRowAnimation?>("AddAnimation");
            TableViewRemoveAnimationMember = AttachedBindingMember.CreateAutoProperty<UITableView, UITableViewRowAnimation?>("RemoveAnimation");
            TableViewReplaceAnimationMember = AttachedBindingMember.CreateAutoProperty<UITableView, UITableViewRowAnimation?>("ReplaceAnimation");
            TableViewScrollPositionMember = AttachedBindingMember.CreateAutoProperty<UITableView, UITableViewScrollPosition?>("ScrollPosition");
            TableViewDefaultCellStyleMember = AttachedBindingMember.CreateAutoProperty<UITableView, UITableViewCellStyle?>("CellStyle");

            //UITableViewCell
            TableViewCellAccessoryButtonTappedEvent = AttachedBindingMember.CreateEvent<UITableViewCell>("AccessoryButtonTapped");
            TableViewCellDeleteClickEvent = AttachedBindingMember.CreateEvent<UITableViewCell>("DeleteClick");
            TableViewCellInsertClickEvent = AttachedBindingMember.CreateEvent<UITableViewCell>("InsertClick");
            TableViewCellMoveableMember = AttachedBindingMember.CreateAutoProperty<UITableViewCell, bool?>("Moveable");
            TitleForDeleteConfirmationMember = AttachedBindingMember.CreateAutoProperty<UITableViewCell, string>("TitleForDeleteConfirmation");
            TableViewCellEditingStyleMember = AttachedBindingMember.CreateAutoProperty<UITableViewCell, UITableViewCellEditingStyle?>("EditingStyle");
            TableViewCellShouldHighlightMember = AttachedBindingMember.CreateAutoProperty<UITableViewCell, bool?>("ShouldHighlight");

            TableViewCellSelectedMember = AttachedBindingMember.CreateNotifiableMember<UITableViewCell, bool>(
                "Selected", (info, cell) =>
                {
                    var cellBindable = cell as UITableViewCellBindable;
                    if (cellBindable == null)
                        return cell.Selected;
                    return cellBindable.SelectedBind;
                }, (info, cell, arg3) =>
                {
                    var cellBindable = cell as UITableViewCellBindable;
                    if (cellBindable == null)
                        cell.Selected = arg3;
                    else
                        cellBindable.SelectedBind = arg3;
                    return true;
                });
            TableViewCellHighlightedMember = AttachedBindingMember.CreateNotifiableMember<UITableViewCell, bool>(
                "Highlighted", (info, cell) => cell.Highlighted,
                (info, cell, arg3) =>
                {
                    if (cell.Highlighted == arg3)
                        return false;
                    cell.Highlighted = arg3;
                    return true;
                });
            TableViewCellEditingMember = AttachedBindingMember.CreateNotifiableMember<UITableViewCell, bool>(
                "Editing", (info, cell) => cell.Editing,
                (info, cell, arg3) =>
                {
                    if (cell.Editing == arg3)
                        return false;
                    cell.Editing = arg3;
                    return true;
                });

            //UICollectionView
            CollectionViewUseAnimationsMember = AttachedBindingMember.CreateAutoProperty<UICollectionView, bool?>("UseAnimations");
            CollectionViewScrollPositionMember = AttachedBindingMember.CreateAutoProperty<UICollectionView, UICollectionViewScrollPosition?>("ScrollPosition");
            CollectionViewSelectedItemChangedEvent = AttachedBindingMember.CreateEvent<UICollectionView>("SelectedItemChanged");

            //UICollectionViewCell
            CollectionViewCellShouldSelectMember = AttachedBindingMember.CreateAutoProperty<UICollectionViewCell, bool?>("ShouldSelect");
            CollectionViewCellShouldDeselectMember = AttachedBindingMember.CreateAutoProperty<UICollectionViewCell, bool?>("ShouldDeselect");
            CollectionViewCellShouldHighlightMember = AttachedBindingMember.CreateAutoProperty<UICollectionViewCell, bool?>("ShouldHighlight");

            CollectionViewCellSelectedMember = AttachedBindingMember
                .CreateNotifiableMember<UICollectionViewCell, bool>("Selected", (info, cell) => cell.Selected,
                    (info, cell, arg3) =>
                    {
                        var cellBindable = cell as UICollectionViewCellBindable;
                        if (cellBindable == null)
                            cell.Selected = arg3;
                        else
                            cellBindable.SelectedBind = arg3;
                        return true;
                    });
            CollectionViewCellHighlightedMember = AttachedBindingMember
                .CreateNotifiableMember<UICollectionViewCell, bool>("Highlighted", (info, cell) => cell.Highlighted,
                    (info, cell, arg3) =>
                    {
                        cell.Highlighted = arg3;
                        return true;
                    });

            //UITabBarController
            TabBarSelectedItemMember = AttachedBindingMember.CreateAutoProperty<UITabBarController, object>(AttachedMemberConstants.SelectedItem, TabBarSelectedItemChanged, TabBarSelectedItemAttached);
        }

        #endregion

        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType("UITextFieldViewMode", typeof(UITextFieldViewMode));
            RegisterTableViewMembers(memberProvider);
            RegisterCollectionViewMembers(memberProvider);
            RegisterDialogMembers(memberProvider);

            //Object
            memberProvider.Register(AttachedBindingMember.CreateMember<object, object>(AttachedMemberConstants.ItemsSource,
                    GetObjectItemsSource, SetObjectItemsSource, ObserveObjectItemsSource));

            memberProvider.Register(CollectionViewManagerMember);
            memberProvider.Register(ContentViewManagerMember);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplate));

            //UIView
            memberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.Parent,
                    (info, view) => ParentObserver.GetOrAdd(view).Parent, null, (info, view, arg3) => ParentObserver.GetOrAdd(view).AddWithUnsubscriber(arg3)));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.FindByNameMethod, FindViewByName));
            memberProvider.Register(ContentMember);
            memberProvider.Register(ContentTemplateMember);
            memberProvider.Register(AttachedBindingMember.CreateMember<UIView, bool>("Visible", (info, view) => !view.Hidden, (info, view, arg3) => view.Hidden = !arg3));

            //UIButton
            memberProvider.Register(AttachedBindingMember.CreateMember<UIButton, string>("Title",
                (info, button) => button.CurrentTitle,
                (info, button, arg3) => button.SetTitle(arg3, UIControlState.Normal)));

            //UIDatePicker
            memberProvider.Register(AttachedBindingMember.CreateMember<UIDatePicker, DateTime>("Date",
                (info, picker) => NSDateToDateTime(picker.Date), (info, picker, arg3) => picker.Date = DateTimeToNSDate(arg3), "ValueChanged"));

            //UISwitch
            memberProvider.Register(AttachedBindingMember.CreateMember<UISwitch, bool>("On",
                (info, picker) => picker.On, (info, picker, arg3) => picker.On = arg3, "ValueChanged"));

            //UIControl
            var clickMember = memberProvider.GetBindingMember(typeof(UIControl), "TouchUpInside", true, false);
            if (clickMember != null)
                memberProvider.Register(typeof(UIControl), "Click", clickMember, true);

            //UITextField
            NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextFieldTextDidChangeNotification, TextDidChangeNotification);
            memberProvider.Register(AttachedBindingMember.CreateEvent<UITextField>("TextChanged", SetTextFieldTextChanged));

            //UITextView
            NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification, TextDidChangeNotification);
            memberProvider.Register(AttachedBindingMember.CreateEvent<UITextView>("TextChanged", SetTextFieldTextChanged));

            //UILabel
            memberProvider.Register(AttachedBindingMember.CreateMember<UILabel, string>("TextSizeToFit",
                (info, label) => label.Text,
                (info, label, arg3) =>
                {
                    label.Text = arg3;
                    label.SizeToFit();
                }));

            //UITabBarController
            memberProvider.Register(TabBarSelectedItemMember);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UITabBarController, IEnumerable>(AttachedMemberConstants.ItemsSource, TabBarItemsSourceChanged));

            //UIViewController
            BindingServiceProvider.BindingMemberPriorities[ToolbarItemTemplate] = 1;
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UIViewController, IEnumerable>("ToolbarItemsSource", ToolbarItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UIViewController, IDataTemplateSelector>(ToolbarItemTemplate));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIViewController, string>("Title",
                (info, controller) => controller.Title,
                (info, controller, arg3) => controller.Title = arg3 ?? string.Empty));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<UIViewController, object>(AttachedMemberConstants.Parent,
                    (info, controller) =>
                    {
                        var parent = BindingExtensions.AttachedParentMember.GetValue(controller, null);
                        if (parent == null)
                            parent = controller.ParentViewController ?? controller.PresentingViewController;
                        return parent;
                    },
                    (info, controller, arg3) => BindingExtensions.AttachedParentMember.SetValue(controller, arg3),
                    (info, controller, arg3) => BindingExtensions.AttachedParentMember.TryObserve(controller, arg3)));

            //UISplitViewController
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UISplitViewController, IEnumerable>(AttachedMemberConstants.ItemsSource, SplitViewControllerItemsSourceChanged));

            //UIToolbar
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UIToolbar, IEnumerable>(AttachedMemberConstants.ItemsSource, ToolbarItemsSourceChanged));

            //UIPickerView
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UIPickerView, IEnumerable>(AttachedMemberConstants.ItemsSource, PickerViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<UIPickerView, string>("DisplayMemberPath", PickerViewDisplayMemberPathChangedChanged));
            memberProvider.Register(
                AttachedBindingMember.CreateMember<UIPickerView, object>(AttachedMemberConstants.SelectedItem,
                    (info, view) => GetOrAddPickerViewModel(view).SelectedItem,
                    (info, view, arg3) => GetOrAddPickerViewModel(view).SelectedItem = arg3, (info, view, arg3) =>
                    {
                        var viewModel = GetOrAddPickerViewModel(view);
                        return BindingServiceProvider.WeakEventManager.TrySubscribe(viewModel, "SelectedItemChanged", arg3);
                    }));
        }


        private static MvvmPickerViewModel GetOrAddPickerViewModel(UIPickerView pickerView)
        {
            var viewModel = pickerView.Model as MvvmPickerViewModel;
            if (viewModel == null)
            {
                viewModel = new MvvmPickerViewModel(pickerView);
                pickerView.Model = viewModel;
            }
            return viewModel;
        }

        private static void PickerViewItemsSourceChanged(UIPickerView pickerView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            GetOrAddPickerViewModel(pickerView).ItemsSource = args.NewValue;
        }

        private static void PickerViewDisplayMemberPathChangedChanged(UIPickerView pickerView, AttachedMemberChangedEventArgs<string> args)
        {
            GetOrAddPickerViewModel(pickerView).DisplayMemberPath = args.NewValue;
        }

        private static void TabBarSelectedItemChanged(UITabBarController bar, AttachedMemberChangedEventArgs<object> args)
        {
            if (args.NewValue == null)
            {
                bar.SelectedIndex = -1;
                return;
            }
            var controllers = bar.ViewControllers;
            if (controllers == null)
                return;
            for (int index = 0; index < controllers.Length; index++)
            {
                var controller = controllers[index];
                if (BindingServiceProvider.ContextManager.GetBindingContext(controller).Value == args.NewValue)
                {
                    bar.SelectedViewController = controller;
                    return;
                }
            }
        }

        private static void TabBarSelectedItemAttached(UITabBarController bar, MemberAttachedEventArgs args)
        {
            bar.ViewControllerSelected += SelecectedControllerChangedHandler;
        }

        private static void TabBarOnViewControllerSelected(object sender, UITabBarSelectionEventArgs args)
        {
            if (args.ViewController == null)
                TabBarSelectedItemMember.SetValue(sender, BindingExtensions.NullValue);
            else
                TabBarSelectedItemMember.SetValue((UITabBarController)sender,
                    BindingServiceProvider.ContextManager.GetBindingContext(args.ViewController).Value);
        }

        private static IItemsSourceGenerator GetOrAddTabBarItemsSourceGenerator(UITabBarController controller)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(controller, TabBarItemsSourceKey, (barController, o) => new ArrayItemsSourceGenerator<UITabBarController, UIViewController>(barController,
                            AttachedMemberConstants.ItemTemplate, (tabBarController, controllers) =>
                            {
                                tabBarController.SetViewControllers(controllers, true);
                                var viewController = tabBarController.SelectedViewController;
                                if (viewController != null)
                                {
                                    if (controllers.Length == 0 || !controllers.Contains(viewController))
                                    {
                                        viewController.RemoveFromParentViewController();
                                        if (viewController.View != null)
                                            viewController.View.RemoveFromSuperviewEx();
                                        if (controllers.Length == 0)
                                            TabBarSelectedItemMember.SetValue(tabBarController, BindingExtensions.NullValue);
                                        else
                                            tabBarController.SelectedViewController = controllers.Last();
                                    }
                                }
                            }), null);
        }

        private static IItemsSourceGenerator GetOrAddToolBarItemsSourceGenerator(UIViewController controller)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(controller, ToolbarItemsSourceKey, (viewController, o) => new ArrayItemsSourceGenerator<UIViewController, UIBarButtonItem>(viewController,
                            ToolbarItemTemplate, (tabBarController, items) => tabBarController.SetToolbarItemsEx(items, true)), null);
        }

        private static IItemsSourceGenerator GetOrAddToolBarItemsSourceGenerator(UIToolbar toolbar)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(toolbar, ItemsSourceGeneratorBase.Key, (viewController, o) => new ArrayItemsSourceGenerator<UIToolbar, UIBarButtonItem>(viewController,
                        AttachedMemberConstants.ItemTemplate, (tabBarController, items) => tabBarController.SetItemsEx(items, true)), null);
        }

        private static void ToolbarItemsSourceChanged(UIToolbar toolbar, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            GetOrAddToolBarItemsSourceGenerator(toolbar).SetItemsSource(args.NewValue);
        }

        private static void ToolbarItemsSourceChanged(UIViewController viewController, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            GetOrAddToolBarItemsSourceGenerator(viewController).SetItemsSource(args.NewValue);
        }

        private static void TabBarItemsSourceChanged(UITabBarController tabBar, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            GetOrAddTabBarItemsSourceGenerator(tabBar).SetItemsSource(args.NewValue);
        }

        private static IDisposable ObserveObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, IEventListener arg3)
        {
            return GetObjectItemsSourceMember(component).TryObserve(component, arg3);
        }

        private static object SetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).SetValue(component, arg3);
        }

        private static object GetObjectItemsSource(IBindingMemberInfo bindingMemberInfo, object component, object[] arg3)
        {
            return GetObjectItemsSourceMember(component).GetValue(component, arg3);
        }

        private static void ObjectItemsSourceChanged(object control, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            ItemsSourceGenerator.GetOrAdd(control).SetItemsSource(args.NewValue);
        }

        private static IBindingMemberInfo GetObjectItemsSourceMember(object component)
        {
            return BindingServiceProvider.MemberProvider.GetBindingMember(component.GetType(),
                AttachedMemberConstants.ItemsSource, true, false) ?? ItemsSourceMember;
        }

        private static IDisposable SetTextFieldTextChanged(IBindingMemberInfo bindingMemberInfo, NSObject item, IEventListener arg3)
        {
            return EventListenerList.GetOrAdd(item, TextChangedEvent).AddWithUnsubscriber(arg3);
        }

        private static object FindViewByName(IBindingMemberInfo bindingMemberInfo, UIView uiView, object[] arg3)
        {
            return FindByName(uiView.GetRootView(), (string)arg3[0]);
        }

        private static UIView FindByName(UIView view, string name)
        {
            if (view == null || view.AccessibilityLabel == name)
                return view;
            foreach (var uiView in view.Subviews)
            {
                view = FindByName(uiView, name);
                if (view != null)
                    return view;
            }
            return null;
        }

        private static void TextDidChangeNotification(NSNotification nsNotification)
        {
            EventListenerList.Raise(nsNotification.Object, TextChangedEvent, EventArgs.Empty);
        }

        private static void ContentTemplateChanged(UIView container, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(container, ContentMember.GetValue(container, null), args.NewValue);
        }

        private static void ContentChanged(UIView container, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(container, args.NewValue, ContentTemplateMember.GetValue(container, null));
        }

        private static void UpdateContent(UIView container, object value, IDataTemplateSelector selector)
        {
            var viewController = ServiceProvider
                .AttachedValueProvider
                .GetValue<UIViewController>(container, ContentControllerPath, false);
            if (viewController != null)
            {
                viewController.RemoveFromParentViewController();
                ServiceProvider.AttachedValueProvider.Clear(container, ContentControllerPath);
                viewController.InititalizeRestorationIdentifier();
            }

            if (selector != null)
                value = selector.SelectTemplateWithContext(value, container);
            var viewModel = value as IViewModel;
            if (viewModel != null)
                value = ViewModelToViewConverter.Instance.Convert(viewModel);

            var view = value as UIView;
            if (view == null)
            {
                viewController = value as UIViewController;
                if (viewController == null)
                {
                    if (value != null)
                        view = new UITextView(new RectangleF(4, 4, 300, 30))
                        {
                            Editable = false,
                            DataDetectorTypes = UIDataDetectorType.None,
                            Text = value.ToString()
                        };
                }
                else
                {
                    var currentController = container.FindParent<UIViewController>();
                    if (currentController != null)
                    {
                        ServiceProvider.AttachedValueProvider.SetValue(container, ContentControllerPath, viewController);
                        viewController.WillMoveToParentViewController(currentController);
                        currentController.AddChildViewController(viewController);
                        viewController.DidMoveToParentViewController(currentController);
                        viewController.RestorationIdentifier = string.Empty;
                        view = viewController.View;
                    }
                }
            }

            IContentViewManager viewManager = ContentViewManagerMember.GetValue(container, null);
            if (viewManager == null)
            {
                container.ClearSubViews();
                if (view != null)
                {
                    view.Frame = container.Frame;
                    view.AutoresizingMask = UIViewAutoresizing.All;
                    container.AddSubviewEx(view);
                }
            }
            else
                viewManager.SetContent(container, view);
        }

        private static void SplitViewControllerItemsSourceChanged(UISplitViewController viewController, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var itemsSource = (IItemsSourceGenerator)ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(viewController, "@!spliitems", (controller, o) => new ArrayItemsSourceGenerator<UISplitViewController, UIViewController>(controller,
                            AttachedMemberConstants.ItemTemplate,
                            (splitViewController, controllers) => splitViewController.ViewControllers = controllers), null);
            itemsSource.SetItemsSource(args.NewValue);
        }

        private static DateTime NSDateToDateTime(NSDate date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        private static NSDate DateTimeToNSDate(DateTime date)
        {
            DateTime reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected override void OnLoaded(IModuleContext context)
        {
            Register(BindingServiceProvider.MemberProvider);
            var converter = new BooleanToCheckmarkAccessoryConverter();
            BindingServiceProvider.ResourceResolver.AddConverter("BooleanToCheckmark", converter);
            BindingServiceProvider.ResourceResolver.AddConverter("BoolToCheckmark", converter);
            base.OnLoaded(context);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}