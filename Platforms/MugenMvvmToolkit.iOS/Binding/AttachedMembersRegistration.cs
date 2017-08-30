#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRegistration.cs">
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
using System.Linq;
using System.Reflection;
using Foundation;
using MonoTouch.Dialog;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.iOS.Binding.Converters;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using MugenMvvmToolkit.iOS.Binding.Models;
using MugenMvvmToolkit.iOS.MonoTouch.Dialog;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding
{
    public static class AttachedMembersRegistration
    {
        #region Fields

        private const string TextChangedEvent = "~@txtchang";
        private const string ContentControllerPath = "#$!contentctr";

        private static readonly EventHandler<UITabBarSelectionEventArgs> SelecectedControllerChangedHandler;
        // ReSharper disable NotAccessedField.Local
        private static NSObject _textObserver;
        private static NSObject _textObserver1;
        // ReSharper restore NotAccessedField.Local

        #endregion

        #region Constructors

        static AttachedMembersRegistration()
        {
            SelecectedControllerChangedHandler = TabBarOnViewControllerSelected;
        }

        #endregion

        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion

        #region Methods

        public static void RegisterObjectMembers()
        {
            var itemsSourceMember = AttachedBindingMember.CreateAutoProperty<object, IEnumerable>(AttachedMemberConstants.ItemsSource, ObjectItemsSourceChanged);
            var defaultMemberRegistration = new DefaultAttachedMemberRegistration<IEnumerable>(itemsSourceMember);
            MemberProvider.Register(defaultMemberRegistration.ToAttachedBindingMember<object>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, ICollectionViewManager>(AttachedMembers.UIView.CollectionViewManager.Path));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<object, IContentViewManager>(AttachedMembers.UIView.ContentViewManager.Path));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    args.NewValue?.SetItemsSource(itemsSource);
                }));

            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty<object, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
            MemberProvider.Register(itemTemplateMember);
            MemberProvider.Register(typeof(object), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);
        }

        public static void RegisterViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.Parent,
                (info, view) => ParentObserver.Get(view), (info, view, arg3) => ParentObserver.Set(view, arg3),
                (info, view, arg3) => ParentObserver.AddListener(view, arg3)));
            MemberProvider.Register(AttachedBindingMember.CreateMember<UIView, object>(AttachedMemberConstants.FindByNameMethod, FindViewByName));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.Content, ContentChanged));
            var member = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ContentTemplateSelector, ContentTemplateChanged);
            MemberProvider.Register(member);
            MemberProvider.Register(typeof(UIView), AttachedMemberConstants.ContentTemplate, member, true);
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIView.Visible, (info, view) => !view.Hidden, (info, view, arg3) => view.Hidden = !arg3));
        }

        public static void RegisterSegmentedControlMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UISegmentedControl>(nameof(UISegmentedControl.SelectedSegment));
            MemberProvider.Register(AttachedBindingMember.CreateMember<UISegmentedControl, int>(nameof(UISegmentedControl.SelectedSegment),
                (info, control) => (int)control.SelectedSegment,
                (info, control, arg3) => control.SelectedSegment = arg3, nameof(UISegmentedControl.ValueChanged)));
        }

        public static void RegisterButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIControl.ClickEvent.Override<UIButton>());
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIButton.Title,
                (info, button) => button.CurrentTitle,
                (info, button, arg3) => button.SetTitle(arg3, UIControlState.Normal)));
        }

        public static void RegisterDatePickerMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIDatePicker.Date);
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIDatePicker.Date,
                (info, picker) => NSDateToDateTime(picker.Date), (info, picker, arg3) => picker.Date = DateTimeToNSDate(arg3), nameof(UIDatePicker.ValueChanged)));
        }

        public static void RegisterSwitchMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UISwitch.On);
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UISwitch.On,
                (info, picker) => picker.On, (info, picker, arg3) => picker.On = arg3, nameof(UISwitch.ValueChanged)));
        }

        public static void RegisterControlMembers()
        {
            var clickMember = MemberProvider.GetBindingMember(typeof(UIControl), nameof(UIControl.TouchUpInside), true, false);
            if (clickMember != null)
                MemberProvider.Register(typeof(UIControl), "Click", clickMember, true);
        }

        public static void RegisterTextFieldMembers()
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITextFieldViewMode));
            BindingBuilderExtensions.RegisterDefaultBindingMember<UITextField>(nameof(UITextField.Text));
            _textObserver = NSNotificationCenter.DefaultCenter.AddObserver(UITextField.TextFieldTextDidChangeNotification, TextDidChangeNotification);
            var memberInfo = AttachedBindingMember.CreateEvent(AttachedMembers.UITextField.TextChangedEvent, SetTextFieldTextChanged);
            AttachedBindingMember.TrySetRaiseAction(memberInfo, RaiseTextChanged);
            MemberProvider.Register(memberInfo);
        }

        public static void RegisterTextViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UITextView>(nameof(UITextView.Text));
            _textObserver1 = NSNotificationCenter.DefaultCenter.AddObserver(UITextView.TextDidChangeNotification, TextDidChangeNotification);
            var memberInfo = AttachedBindingMember.CreateEvent(AttachedMembers.UITextView.TextChangedEvent, SetTextFieldTextChanged);
            AttachedBindingMember.TrySetRaiseAction(memberInfo, RaiseTextChanged);
            MemberProvider.Register(memberInfo);
        }

        public static void RegisterLabelMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UILabel>(nameof(UILabel.Text));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UILabel.TextSizeToFit,
                (info, label) => label.Text,
                (info, label, arg3) =>
                {
                    label.Text = arg3;
                    label.SizeToFit();
                }));
        }

        public static void RegisterBaseViewControllerMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<UIViewController, string>(nameof(UIViewController.Title),
                (info, controller) => controller.Title,
                (info, controller, arg3) => controller.Title = arg3 ?? string.Empty));
            MemberProvider.Register(AttachedBindingMember.CreateMember<UIViewController, object>(AttachedMemberConstants.ParentExplicit,
                (info, controller) => controller.ParentViewController ?? controller.PresentingViewController ?? controller.SplitViewController, null));
        }

        public static void RegisterViewControllerMembers()
        {
            BindingServiceProvider.BindingMemberPriorities["ToolbarItemTemplate"] = BindingServiceProvider.TemplateMemberPriority;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UIViewController.ToolbarItemTemplateSelector] = BindingServiceProvider.TemplateMemberPriority;
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIViewController.ToastTemplateSelector));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIViewController.ToolbarItemsSource, ToolbarItemsSourceChanged));
            var templateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIViewController.ToolbarItemTemplateSelector);
            MemberProvider.Register(templateMember);
            MemberProvider.Register(typeof(UIViewController), "ToolbarItemTemplate", templateMember, true);
        }

        public static void RegisterTabBarControllerMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UITabBarController.ItemsSource);
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITabBarController.SelectedItem, TabBarSelectedItemChanged, TabBarSelectedItemAttached));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITabBarController.ItemsSource, TabBarItemsSourceChanged));
        }

        public static void RegisterSplitViewControllerMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.UISplitViewController.MasterView,
                (info, controller) =>
                {
                    if (controller.ViewControllers.Length == 2)
                        return controller.ViewControllers[0];
                    return null;
                }, (info, controller, arg3) =>
                {
                    UpdateMasterDetailController(controller, arg3, true);
                    return true;
                }));
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.UISplitViewController.DetailView,
                (info, controller) =>
                {
                    if (controller.ViewControllers.Length == 2)
                        return controller.ViewControllers[1];
                    return null;
                }, (info, controller, arg3) =>
                {
                    UpdateMasterDetailController(controller, arg3, false);
                    return true;
                }));
        }

        public static void RegisterToolbarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UIToolbar>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UIToolbar>(), ToolbarItemsSourceChanged));
        }

        public static void RegisterPickerViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UIPickerView>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UIPickerView>(), PickerViewItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIPickerView.DisplayMemberPath, PickerViewDisplayMemberPathChangedChanged));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIPickerView.SelectedItem,
                (info, view) => GetOrAddPickerViewModel(view).SelectedItem,
                (info, view, arg3) => GetOrAddPickerViewModel(view).SelectedItem = arg3, (info, view, arg3) =>
                {
                    var viewModel = GetOrAddPickerViewModel(view);
                    return BindingServiceProvider.WeakEventManager.TrySubscribe(viewModel, nameof(MvvmPickerViewModel.SelectedItemChanged), arg3);
                }));
        }

        public static void RegisterBarButtonItemMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIBarButtonItem.ClickEvent);
        }

        public static void RegisterSearchBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UISearchBar>(nameof(UISearchBar.Text));
        }

        public static void RegisterSliderMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UISlider>(nameof(UISlider.Value));
        }

        public static void RegisterProgressViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UIProgressView>(nameof(UIProgressView.Progress));
        }

        public static void RegisterDialogElementMembers()
        {
            DefaultCollectionViewManager.InsertInternalHandler = (view, index, item) =>
            {
                var section = view as Section;
                if (section != null)
                {
                    section.Insert(index, (Element)item);
                    ((Element)item).RaiseParentChanged();
                    return true;
                }

                var rootElement = view as RootElement;
                if (rootElement != null)
                {
                    rootElement.Insert(index, (Section)item);
                    ((Section)item).RaiseParentChanged();
                    return true;
                }
                return false;
            };
            DefaultCollectionViewManager.RemoveAtInternalHandler = (view, index) =>
            {
                var section = view as Section;
                if (section != null)
                {
                    var element = section[index];
                    section.Remove(index);
                    element.ClearBindingsRecursively(true, true);
                    element.DisposeEx();
                    return true;
                }

                var rootElement = view as RootElement;
                if (rootElement != null)
                {
                    var element = rootElement[index];
                    rootElement.RemoveAt(index);
                    element.ClearBindingsRecursively(true, true);
                    element.DisposeEx();
                    return true;
                }
                return false;
            };
            DefaultCollectionViewManager.ClearInternalHandler = view =>
            {
                var section = view as Section;
                if (section != null)
                {
                    var elements = section.OfType<Element>().ToArray();
                    section.Clear();
                    foreach (var element in elements)
                    {
                        element.ClearBindingsRecursively(true, true);
                        element.DisposeEx();
                    }
                    return true;
                }

                var rootElement = view as RootElement;
                if (rootElement != null)
                {
                    var elements = rootElement.ToArray();
                    rootElement.Clear();
                    foreach (var element in elements)
                    {
                        element.ClearBindingsRecursively(true, true);
                        element.DisposeEx();
                    }
                    return true;
                }
                return false;
            };

            BindingBuilderExtensions.RegisterDefaultBindingMember<Element>(nameof(Element.Caption));
            MemberProvider.Register(AttachedBindingMember.CreateMember<Element, string>(nameof(Element.Caption),
                (info, element) => element.Caption,
                (info, element, arg3) =>
                {
                    element.Caption = arg3;
                    element.Reload();
                }));
            MemberProvider.Register(AttachedBindingMember.CreateMember<Element, object>(AttachedMemberConstants.ParentExplicit,
                (info, element) => element.Parent, null));
        }

        public static void RegisterDialogEntryElementMembers()
        {
            var field = typeof(EntryElement).GetField("entry", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null && field.FieldType == typeof(UITextField))
            {
                var getEntryField = ServiceProvider.ReflectionManager.GetMemberGetter<UITextField>(field);
                TouchBindingErrorProvider.TryGetEntryField = target =>
                {
                    var element = target as EntryElement;
                    if (element != null)
                        target = getEntryField(element);
                    return target;
                };
            }

            BindingBuilderExtensions.RegisterDefaultBindingMember<EntryElement>(nameof(EntryElement.Value));
            var member = MemberProvider.GetBindingMember(typeof(EntryElement), nameof(EntryElement.Changed), true, false);
            if (member != null)
                MemberProvider.Register(AttachedBindingMember.CreateEvent<EntryElement>("ValueChanged",
                    (info, element, arg3) => member.TryObserve(element, arg3)));
        }

        public static void RegisterStringElementMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<StringElement>(nameof(StringElement.Value));
            MemberProvider.Register(AttachedBindingMember.CreateMember<StringElement, string>(nameof(StringElement.Value),
                (info, element) => element.Value,
                (info, element, arg3) =>
                {
                    element.Value = arg3;
                    element.Reload();
                }));
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.StringElement.TappedEvent,
                (info, element, arg3) =>
                {
                    var weakWrapper = arg3.ToWeakWrapper();
                    IDisposable unsubscriber = null;
                    Action action = () =>
                    {
                        if (!weakWrapper.EventListener.TryHandle(weakWrapper.EventListener, EventArgs.Empty))
                            unsubscriber.Dispose();
                    };
                    unsubscriber = WeakActionToken.Create(element, action,
                        (stringElement, nsAction) => stringElement.Tapped -= nsAction);
                    element.Tapped += action;
                    return unsubscriber;
                }));
        }

        public static void RegisterTableViewMembers()
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewRowAnimation));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewScrollPosition));

            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.UseAnimations] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.AddAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.RemoveAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.ReplaceAnimation] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UITableView.ScrollPosition] = BindingServiceProvider.TemplateMemberPriority + 1;

            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UITableView>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.AddAnimation));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.RemoveAnimation));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ReplaceAnimation));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ScrollPosition));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.UseAnimations));
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableView.SelectedItemChangedEvent));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UITableView>(), TableViewItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UITableView.SelectedItem,
                GetTableViewSelectedItem, SetTableViewSelectedItem,
                (info, view, arg3) => (IDisposable)view.SetBindingMemberValue(AttachedMembers.UITableView.SelectedItemChangedEvent, arg3)));
            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableView.ItemTemplateSelector, (view, args) => args.NewValue?.Initialize(view));
            MemberProvider.Register(itemTemplateMember);
            MemberProvider.Register(typeof(UITableView), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);
        }

        public static void RegisterTableViewCellMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<UITableViewCell>(() => c => c.TextLabel.Text);
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellEditingStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellAccessory));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellSelectionStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellSeparatorStyle));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UITableViewCellStyle));
            var member = AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.AccessoryButtonTappedEvent);
            MemberProvider.Register(member);
            MemberProvider.Register("Click", member);
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.DeleteClickEvent));
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UITableViewCell.InsertClickEvent));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.TitleForDeleteConfirmation));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UITableViewCell.EditingStyle));
            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateMember<UITableViewCell, bool?>(nameof(UITableViewCell.Selected),
                (info, target) => TableViewSourceBase.CellMediator.GetMediator(target, true).SelectedBind,
                (info, cell, arg3) => TableViewSourceBase.CellMediator.GetMediator(cell, true).SelectedBind = arg3,
                (info, cell, arg3) => TableViewSourceBase.CellMediator.GetMediator(cell, true).AddWithUnsubscriber(arg3)));
        }

        public static void RegisterCollectionViewMembers(bool isDataSource)
        {
            BindingServiceProvider.ResourceResolver.AddType(typeof(UICollectionViewScrollPosition));
            BindingServiceProvider.ResourceResolver.AddType(typeof(UICollectionViewScrollDirection));

            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UICollectionView.UseAnimations] = BindingServiceProvider.TemplateMemberPriority + 1;
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.UICollectionView.ScrollPosition] = BindingServiceProvider.TemplateMemberPriority + 1;

            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.UIView.ItemsSource.Override<UICollectionView>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.UseAnimations));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.ScrollPosition));
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.UICollectionView.SelectedItemChangedEvent));
            MemberProvider.Register(isDataSource
                ? AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UICollectionView>(), CollectionViewItemsSourceChangedDataSource)
                : AttachedBindingMember.CreateAutoProperty(AttachedMembers.UIView.ItemsSource.Override<UICollectionView>(), CollectionViewItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UICollectionView.SelectedItem,
                GetCollectionViewSelectedItem, SetCollectionViewSelectedItem,
                (info, view, arg3) => (IDisposable)view.SetBindingMemberValue(AttachedMembers.UICollectionView.SelectedItemChangedEvent, arg3)));
            var itemTemplateMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.UICollectionView.ItemTemplateSelector, (view, args) => args.NewValue?.Initialize(view));
            MemberProvider.Register(itemTemplateMember);
            MemberProvider.Register(typeof(UICollectionView), AttachedMemberConstants.ItemTemplate, itemTemplateMember, true);
        }

        public static void RegisterCollectionViewCellMembers()
        {
            BindingServiceProvider.MemberProvider.Register(AttachedBindingMember.CreateMember<UICollectionViewCell, bool?>(nameof(UICollectionViewCell.Selected),
                (info, target) => CollectionViewSourceBase.CellMediator.GetMediator(target, true).SelectedBind,
                (info, cell, arg3) => CollectionViewSourceBase.CellMediator.GetMediator(cell, true).SelectedBind = arg3,
                (info, cell, arg3) => CollectionViewSourceBase.CellMediator.GetMediator(cell, true).AddWithUnsubscriber(arg3)));
        }

        private static void SetTableViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UITableView uiTableView, object arg3)
        {
            var tableViewSource = uiTableView.Source as TableViewSourceBase;
            if (tableViewSource != null)
                tableViewSource.SelectedItem = arg3;
        }

        private static object GetTableViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UITableView uiTableView)
        {
            return (uiTableView.Source as TableViewSourceBase)?.SelectedItem;
        }

        private static void TableViewItemsSourceChanged(UITableView uiTableView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            if (uiTableView.Source == null)
                uiTableView.Source = TouchToolkitExtensions.TableViewSourceFactory(uiTableView, DataContext.Empty);
            var tableViewSource = uiTableView.Source as ItemsSourceTableViewSource;
            if (tableViewSource != null)
                tableViewSource.ItemsSource = args.NewValue;
        }

        private static void SetCollectionViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UICollectionView collectionView, object arg3)
        {
            var source = collectionView.Source as CollectionViewSourceBase;
            if (source != null)
                source.SelectedItem = arg3;
        }

        private static object GetCollectionViewSelectedItem(IBindingMemberInfo bindingMemberInfo, UICollectionView collectionView)
        {
            return (collectionView.Source as CollectionViewSourceBase)?.SelectedItem;
        }

        private static void CollectionViewItemsSourceChanged(UICollectionView collectionView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            if (collectionView.Source == null)
                collectionView.Source = TouchToolkitExtensions.CollectionViewSourceFactory(collectionView, DataContext.Empty);
            var source = collectionView.Source as ItemsSourceCollectionViewSource;
            if (source != null)
                source.ItemsSource = args.NewValue;
        }

        private static void CollectionViewItemsSourceChangedDataSource(UICollectionView collectionView, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            if (collectionView.DataSource == null)
                collectionView.DataSource = TouchToolkitExtensions.CollectionViewSourceFactory(collectionView, DataContext.Empty);
            var source = collectionView.DataSource as ItemsSourceCollectionViewSource;
            if (source != null)
                source.ItemsSource = args.NewValue;
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
                    TouchToolkitExtensions.SetHasState(viewController, false);
                    value = viewController.View;
                }
            }


            var viewManager = container.GetBindingMemberValue(AttachedMembers.UIView.ContentViewManager);
            if (viewManager == null)
            {
                container.ClearSubViews();
                var view = value as UIView;
                if ((view == null) && (value != null))
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

        private static object FindViewByName(IBindingMemberInfo bindingMemberInfo, UIView uiView, object[] arg3)
        {
            return FindByName(uiView.GetRootView(), (string)arg3[0]);
        }

        private static UIView FindByName(UIView view, string name)
        {
            if ((view == null) || (view.AccessibilityLabel == name))
                return view;
            if (view.Subviews != null)
                foreach (var uiView in view.Subviews)
                {
                    view = FindByName(uiView, name);
                    if (view != null)
                        return view;
                }
            return null;
        }

        private static void ContentTemplateChanged(UIView container, AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(container, container.GetBindingMemberValue(AttachedMembers.UIView.Content), args.NewValue);
        }

        private static void ContentChanged(UIView container, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(container, args.NewValue, container.GetBindingMemberValue(AttachedMembers.UIView.ContentTemplateSelector));
        }

        private static DateTime NSDateToDateTime(NSDate date)
        {
            var reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return reference.AddSeconds(date.SecondsSinceReferenceDate);
        }

        private static NSDate DateTimeToNSDate(DateTime date)
        {
            var reference = TimeZone.CurrentTimeZone.ToLocalTime(
                new DateTime(2001, 1, 1, 0, 0, 0));
            return NSDate.FromTimeIntervalSinceReferenceDate(
                (date - reference).TotalSeconds);
        }

        private static IDisposable SetTextFieldTextChanged(IBindingMemberInfo bindingMemberInfo, NSObject item, IEventListener arg3)
        {
            return EventListenerList.GetOrAdd(item, TextChangedEvent).AddWithUnsubscriber(arg3);
        }

        private static void RaiseTextChanged(IBindingMemberInfo info, object field, object arg3)
        {
            EventListenerList.GetOrAdd(field, TextChangedEvent).Raise(field, arg3);
        }

        private static void TextDidChangeNotification(NSNotification nsNotification)
        {
            EventListenerList.Raise(nsNotification.Object, TextChangedEvent, EventArgs.Empty);
        }

        private static void UpdateMasterDetailController(UISplitViewController splitView, UIViewController newValue, bool isMaster)
        {
            if (newValue == null)
                newValue = new UIViewController();
            else
                TouchToolkitExtensions.SetHasState(newValue, false);
            var viewControllers = splitView.ViewControllers ?? Empty.Array<UIViewController>();
            if (viewControllers.Length == 2)
                if (isMaster)
                {
                    if (!ReferenceEquals(viewControllers[0], newValue))
                        splitView.ViewControllers = new[] { newValue, viewControllers[1] };
                }
                else
                {
                    if (!ReferenceEquals(viewControllers[1], newValue))
                        splitView.ViewControllers = new[] { viewControllers[0], newValue };
                }
            else
                splitView.ViewControllers = isMaster
                    ? new[] { newValue, new UIViewController() }
                    : new[] { new UIViewController(), newValue };
            for (var i = 0; i < viewControllers.Length; i++)
                viewControllers[i].TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
            newValue.TryRaiseAttachedEvent(AttachedMembers.Object.Parent);
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
            for (var index = 0; index < controllers.Length; index++)
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
            var generator = controller.GetBindingMemberValue(AttachedMembers.UITabBarController.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new ArrayItemsSourceGenerator<UITabBarController, UIViewController>(controller,
                    AttachedMemberConstants.ItemTemplate, (tabBarController, controllers) =>
                    {
                        tabBarController.SetViewControllers(controllers, true);
                        var viewController = tabBarController.SelectedViewController;
                        if (viewController != null)
                            if ((controllers.Length == 0) || !controllers.Contains(viewController))
                            {
                                viewController.RemoveFromParentViewController();
                                viewController.View?.RemoveFromSuperviewEx();
                                if (controllers.Length == 0)
                                    tabBarController.SetBindingMemberValue(
                                        AttachedMembers.UITabBarController.SelectedItem, BindingExtensions.NullValue);
                                else
                                    tabBarController.SelectedViewController = controllers.Last();
                            }
                    });
                controller.SetBindingMemberValue(AttachedMembers.UITabBarController.ItemsSourceGenerator, generator);
            }
            return generator;
        }

        private static IItemsSourceGenerator GetOrAddToolBarItemsSourceGenerator(UIViewController controller)
        {
            var generator = controller.GetBindingMemberValue(AttachedMembers.UIViewController.ToolbarItemsSourceGenerator);
            if (generator == null)
            {
                generator = new ArrayItemsSourceGenerator<UIViewController, UIBarButtonItem>(controller,
                    AttachedMembers.UIViewController.ToolbarItemTemplateSelector,
                    (tabBarController, items) => tabBarController.SetToolbarItemsEx(items, true));
                controller.SetBindingMemberValue(AttachedMembers.UIViewController.ToolbarItemsSourceGenerator, generator);
            }
            return generator;
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

        #endregion
    }
}