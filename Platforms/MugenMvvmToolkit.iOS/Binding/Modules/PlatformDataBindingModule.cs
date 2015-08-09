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
using System.Linq;
using CoreGraphics;
using Foundation;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.iOS.Binding.Converters;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using MugenMvvmToolkit.iOS.Binding.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Modules
{
    public partial class PlatformDataBindingModule : DataBindingModule
    {
        #region Fields

        private static readonly EventHandler<UITabBarSelectionEventArgs> SelecectedControllerChangedHandler;

        private const string TabBarItemsSourceKey = "~~@!tabitems";
        private const string ToolbarItemsSourceKey = "~~@!toolbaritems";
        private const string TextChangedEvent = "~@txtchang";
        private const string ContentControllerPath = "#$!contentctr";

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            SelecectedControllerChangedHandler = TabBarOnViewControllerSelected;
        }

        #endregion

        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITextFieldViewMode));
            RegisterTableViewMembers(memberProvider);
            RegisterCollectionViewMembers(memberProvider);
            RegisterDialogMembers(memberProvider);

            //Object
            var itemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            var defaultMemberRegistration = new DefaultAttachedMemberRegistration<IEnumerable>(itemsSourceMember);
            memberProvider.Register(defaultMemberRegistration.ToAttachedBindingMember<object>());
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>(AttachedMembers.UIView.CollectionViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>(AttachedMembers.UIView.ContentViewManager.Path));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    if (args.NewValue != null)
                        args.NewValue.SetItemsSource(itemsSource);
                }));

            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
            memberProvider.Register(itemTemplateMember);
            memberProvider.Register(typeof(object), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);

            //UIView
            memberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.ParentExplicit,
                (info, view) => ParentObserver.GetOrAdd(view).Parent, null, (info, view, arg3) => ParentObserver.GetOrAdd(view).AddWithUnsubscriber(arg3)));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.FindByNameMethod, FindViewByName));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.Content, ContentChanged));
            var member = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ContentTemplateSelector, ContentTemplateChanged);
            memberProvider.Register(member);
            memberProvider.Register(typeof(UIView), AttachedMemberConstants.ContentTemplate, member, true);
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIView.Visible, (info, view) => !view.Hidden, (info, view, arg3) => view.Hidden = !arg3));

            //UIButton
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIButton.Title,
                (info, button) => button.CurrentTitle,
                (info, button, arg3) => button.SetTitle(arg3, UIControlState.Normal)));

            //UIDatePicker
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIDatePicker.Date,
                (info, picker) => NSDateToDateTime(picker.Date), (info, picker, arg3) => picker.Date = DateTimeToNSDate(arg3), "ValueChanged"));

            //UISwitch
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UISwitch.On,
                (info, picker) => picker.On, (info, picker, arg3) => picker.On = arg3, "ValueChanged"));

            //UIControl
            var clickMember = memberProvider.GetBindingMember(typeof(UIControl), "TouchUpInside", true, false);
            if (clickMember != null)
                memberProvider.Register(typeof(UIControl), "Click", clickMember, true);

            //UITextField
            NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextFieldTextDidChangeNotification, TextDidChangeNotification);
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITextField.TextChangedEvent, SetTextFieldTextChanged));

            //UITextView
            NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification, TextDidChangeNotification);
            memberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITextView.TextChangedEvent, SetTextFieldTextChanged));

            //UILabel
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UILabel.TextSizeToFit,
                (info, label) => label.Text,
                (info, label, arg3) =>
                {
                    label.Text = arg3;
                    label.SizeToFit();
                }));

            //UIViewController
            BindingServiceProvider.BindingMemberPriorities["ToolbarItemTemplate"] = 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UIViewController.ToolbarItemTemplateSelector] = 1;
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIViewController.ToolbarItemsSource, ToolbarItemsSourceChanged));
            var templateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIViewController.ToolbarItemTemplateSelector);
            memberProvider.Register(templateMember);
            memberProvider.Register(typeof(UIViewController), "ToolbarItemTemplate", templateMember, true);
            memberProvider.Register(AttachedBindingMember.CreateMember<UIViewController, string>("Title",
                (info, controller) => controller.Title,
                (info, controller, arg3) => controller.Title = arg3 ?? string.Empty));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIViewController, object>(AttachedMemberConstants.ParentExplicit,
                    (info, controller) => controller.ParentViewController ?? controller.PresentingViewController, null));

            //UITabBarController
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITabBarController.SelectedItem, TabBarSelectedItemChanged, TabBarSelectedItemAttached));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITabBarController.ItemsSource, TabBarItemsSourceChanged));

            //UISplitViewController
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UISplitViewController.ItemsSource, SplitViewControllerItemsSourceChanged));

            //UIToolbar
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UIToolbar>(), ToolbarItemsSourceChanged));

            //UIPickerView
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UIPickerView>(), PickerViewItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIPickerView.DisplayMemberPath, PickerViewDisplayMemberPathChangedChanged));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIPickerView.SelectedItem,
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
                if (controller.DataContext() == args.NewValue)
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
            var tabBarController = (UITabBarController)sender;
            if (args.ViewController == null)
                tabBarController.SetBindingMemberValue(AttachedMembers.UITabBarController.SelectedItem, BindingExtensions.NullValue);
            else
                tabBarController.SetBindingMemberValue(AttachedMembers.UITabBarController.SelectedItem, args.ViewController.DataContext());
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
                                            tabBarController.SetBindingMemberValue(AttachedMembers.UITabBarController.SelectedItem, BindingExtensions.NullValue);
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
                            AttachedMembers.UIViewController.ToolbarItemTemplateSelector, (tabBarController, items) => tabBarController.SetToolbarItemsEx(items, true)), null);
        }

        private static IItemsSourceGenerator GetOrAddToolBarItemsSourceGenerator(UIToolbar toolbar)
        {
            var generator = toolbar.GetBindingMemberValue(AttachedMembers.UIView.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new ArrayItemsSourceGenerator<UIToolbar, UIBarButtonItem>(toolbar,
                   AttachedMemberConstants.ItemTemplate,
                   (tabBarController, items) => tabBarController.SetItemsEx(items, true));
                toolbar.SetBindingMemberValue(AttachedMembers.UIView.ItemsSourceGenerator, generator);
            }
            return generator;
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

        private static void ObjectItemsSourceChanged(object control, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = control.GetBindingMemberValue(ItemsSourceGeneratorBase.MemberDescriptor);
            if (generator == null)
            {
                generator = new ItemsSourceGenerator(control);
                control.SetBindingMemberValue(ItemsSourceGeneratorBase.MemberDescriptor, generator);
            }
            generator.SetItemsSource(args.NewValue);
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
            if (view.Subviews != null)
            {
                foreach (var uiView in view.Subviews)
                {
                    view = FindByName(uiView, name);
                    if (view != null)
                        return view;
                }
            }
            return null;
        }

        private static void TextDidChangeNotification(NSNotification nsNotification)
        {
            EventListenerList.Raise(nsNotification.Object, TextChangedEvent, EventArgs.Empty);
        }

        private static void ContentTemplateChanged(UIView container, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(container, container.GetBindingMemberValue(AttachedMembers.UIView.Content), args.NewValue);
        }

        private static void ContentChanged(UIView container, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(container, args.NewValue, container.GetBindingMemberValue(AttachedMembers.UIView.ContentTemplateSelector));
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

            viewController = value as UIViewController;
            if (viewController != null)
            {
                var currentController = container.FindParent<UIViewController>();
                if (currentController != null)
                {
                    ServiceProvider.AttachedValueProvider.SetValue(container, ContentControllerPath, viewController);
                    viewController.WillMoveToParentViewController(currentController);
                    currentController.AddChildViewController(viewController);
                    viewController.DidMoveToParentViewController(currentController);
                    viewController.RestorationIdentifier = string.Empty;
                    value = viewController.View;
                }
            }


            var viewManager = container.GetBindingMemberValue(AttachedMembers.UIView.ContentViewManager);
            if (viewManager == null)
            {
                container.ClearSubViews();
                var view = value as UIView;
                if (view == null && value != null)
                    view = new UITextView(container.Frame)
                    {
                        Editable = false,
                        DataDetectorTypes = UIDataDetectorType.None,
                        Text = value.ToString()
                    };
                if (view != null)
                {
                    view.Frame = container.Frame;
                    view.AutoresizingMask = UIViewAutoresizing.All;
                    container.AddSubviewEx(view);
                }
            }
            else
                viewManager.SetContent(container, value);
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
        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}