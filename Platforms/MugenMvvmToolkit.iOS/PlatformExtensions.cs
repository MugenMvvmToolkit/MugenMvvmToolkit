#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Models;
using MugenMvvmToolkit.iOS.Infrastructure;
using MugenMvvmToolkit.iOS.Infrastructure.Mediators;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.Models;
using ObjCRuntime;
using UIKit;

namespace MugenMvvmToolkit.iOS
{
    public static partial class PlatformExtensions
    {
        #region Nested types

        private sealed class ActionSheetButtonClosure
        {
            #region Fields

            public nint Index;

            #endregion

            #region Properties

            public bool Enabled
            {
                get { return true; }
                // ReSharper disable once ValueParameterNotUsed
                set {; }
            }

            #endregion

            #region Events

            public event EventHandler Click;

            #endregion

            #region Methods

            public void OnClick(object sender, UIButtonEventArgs e)
            {
                if (e.ButtonIndex != Index)
                    return;
                var handler = Click;
                if (handler != null)
                    handler(sender, e);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string NavParamKey = "@~`NavParam";
        private const string NoStateKey = "@$Nst";

        private static readonly Dictionary<Type, int> TypeToCounters;
        private static readonly Type[] CoderParameters;

        private static IApplicationStateManager _applicationStateManager;
        private static Func<UIViewController, IDataContext, IMvvmViewControllerMediator> _mvvmViewControllerMediatorFactory;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            TypeToCounters = new Dictionary<Type, int>();
            CoderParameters = new[] { typeof(NSCoder) };
            _mvvmViewControllerMediatorFactory = (controller, context) => new MvvmViewControllerMediator(controller);
        }

        #endregion

        #region Properties

        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    Interlocked.CompareExchange(ref _applicationStateManager, ServiceProvider.Get<IApplicationStateManager>(), null);
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

        [NotNull]
        public static Func<UIViewController, IDataContext, IMvvmViewControllerMediator> MvvmViewControllerMediatorFactory
        {
            get { return _mvvmViewControllerMediatorFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _mvvmViewControllerMediatorFactory = value;
            }
        }

        #endregion

        #region Methods

        public static void ShowEx<T, TParent>([NotNull] this T actionSheet, TParent parent,
            [NotNull] Action<T, TParent> showAction)
            where T : UIActionSheet
        {
            Should.NotBeNull(actionSheet, "actionSheet");
            Should.NotBeNull(showAction, "showAction");
            ParentObserver.GetOrAdd(actionSheet).Parent = parent;
            showAction(actionSheet, parent);
        }

        public static void AddButtonWithBinding(this UIActionSheet actionSheet, string title, [NotNull] string bindingExpression, IList<object> sources = null)
        {
            Should.NotBeNull(actionSheet, "actionSheet");
            Should.NotBeNull(bindingExpression, "bindingExpression");
            if (IsOS8)
                actionSheet.AddButtonOS8(title, bindingExpression, sources);
            else
                actionSheet.AddButtonOS7(title, bindingExpression, sources);
        }

        public static void SetInputViewEx([NotNull] this UITextField textField, UIView view)
        {
            Should.NotBeNull(textField, "textField");
            textField.InputView = view;
            if (view != null)
                ParentObserver.GetOrAdd(view).Parent = textField;
        }

        public static void SetNavigationParameter([NotNull] this UIViewController controller, object value)
        {
            Should.NotBeNull(controller, "controller");
            if (value == null)
                ServiceProvider.AttachedValueProvider.Clear(controller, NavParamKey);
            else
                ServiceProvider.AttachedValueProvider.SetValue(controller, NavParamKey, value);
        }

        public static object GetNavigationParameter([CanBeNull] this UIViewController controller)
        {
            if (controller == null)
                return null;
            return ServiceProvider.AttachedValueProvider.GetValue<string>(controller, NavParamKey, false);
        }

        public static void SetCellBind([NotNull] this UITableView tableView,
            [CanBeNull] Action<UITableViewCell> bindAction)
        {
            Should.NotBeNull(tableView, "tableView");
            tableView.SetBindingMemberValue(AttachedMembers.UITableView.CellBind, bindAction);
        }

        public static void SetCellStyle([NotNull] this UITableView tableView, UITableViewCellStyle style)
        {
            Should.NotBeNull(tableView, "tableView");
            tableView.SetBindingMemberValue(AttachedMembers.UITableView.CellStyle, style);
        }

        public static UITableViewCell CellAtEx(this UITableView tableView, NSIndexPath indexPath)
        {
            Should.NotBeNull(tableView, "tableView");
            var sourceBase = tableView.Source as TableViewSourceBase;
            if (sourceBase == null)
                return tableView.CellAt(indexPath);
            return sourceBase.CellAt(tableView, indexPath);
        }

        public static NSIndexPath IndexPathForCellEx(this UITableView tableView, UITableViewCell cell)
        {
            Should.NotBeNull(tableView, "tableView");
            var sourceBase = tableView.Source as TableViewSourceBase;
            if (sourceBase == null)
                return tableView.IndexPathForCell(cell);
            return sourceBase.IndexPathForCell(tableView, cell);
        }

        public static UICollectionViewCell CellForItemEx(this UICollectionView collectionView, NSIndexPath indexPath)
        {
            Should.NotBeNull(collectionView, "collectionView");
            var sourceBase = collectionView.Source as CollectionViewSourceBase;
            if (sourceBase == null)
                return collectionView.CellForItem(indexPath);
            return sourceBase.CellForItem(collectionView, indexPath);
        }

        public static NSIndexPath IndexPathForCellEx(this UICollectionView collectionView, UICollectionViewCell cell)
        {
            Should.NotBeNull(collectionView, "collectionView");
            var sourceBase = collectionView.Source as CollectionViewSourceBase;
            if (sourceBase == null)
                return collectionView.IndexPathForCell(cell);
            return sourceBase.IndexPathForCell(collectionView, cell);
        }

        public static void SetEditingStyle([NotNull]this UITableViewCell cell, UITableViewCellEditingStyle editingStyle)
        {
            Should.NotBeNull(cell, "cell");
            cell.SetBindingMemberValue(AttachedMembers.UITableViewCell.EditingStyle, editingStyle);
        }

        public static void SetToolbarItemsEx([NotNull] this UIViewController controller, UIBarButtonItem[] items,
            bool? animated = null)
        {
            Should.NotBeNull(controller, "controller");
            SetParent(items, controller);
            if (animated == null)
                controller.ToolbarItems = items;
            else
                controller.SetToolbarItems(items, animated.Value);
        }

        public static void SetItemsEx([NotNull] this UIToolbar toolbar, UIBarButtonItem[] items, bool? animated = null)
        {
            Should.NotBeNull(toolbar, "toolbar");
            SetParent(items, toolbar);
            if (animated == null)
                toolbar.Items = items;
            else
                toolbar.SetItems(items, animated.Value);
        }

        public static void SetItemEx<T, TItem>([NotNull] this T container, Action<T, TItem> setAction, TItem item)
            where T : INativeObject
            where TItem : class
        {
            Should.NotBeNull(container, "container");
            item.SetBindingMemberValue(AttachedMembers.Object.Parent, container);
            setAction(container, item);
        }

        public static void SetItemsEx<T, TItem>([NotNull] this T container, Action<T, TItem[]> setAction, params TItem[] items)
            where T : INativeObject
            where TItem : class
        {
            Should.NotBeNull(container, "container");
            SetParent(items, container);
            setAction(container, items);
        }

        public static void RaiseParentChanged(this UIView uiView, bool recursively = true)
        {
            ParentObserver.Raise(uiView, recursively);
        }

        public static void AddEx([NotNull] this UIViewController controller, UIView view)
        {
            Should.NotBeNull(controller, "controller");
            controller.Add(view);
            view.RaiseParentChanged();
        }

        public static void AddEx([NotNull] this UIView parent, UIView view)
        {
            Should.NotBeNull(parent, "parent");
            parent.Add(view);
            view.RaiseParentChanged();
        }

        public static void AddSubviewEx([NotNull] this UIView parent, UIView view)
        {
            Should.NotBeNull(parent, "parent");
            parent.AddSubview(view);
            view.RaiseParentChanged();
        }

        public static void AddSubviewsEx([NotNull] this UIView parent, params UIView[] subViews)
        {
            Should.NotBeNull(parent, "parent");
            parent.AddSubviews(subViews);
            RaiseParentChanged(subViews);
        }

        public static void InsertSubviewEx([NotNull] this UIView parent, UIView view, int index)
        {
            Should.NotBeNull(parent, "parent");
            parent.InsertSubview(view, index);
            view.RaiseParentChanged();
        }

        public static void RemoveFromSuperviewEx([NotNull] this UIView view)
        {
            Should.NotBeNull(view, "view");
            view.RemoveFromSuperview();
            view.RaiseParentChanged(false);
        }

        public static void ClearSubViews(this UIView view)
        {
            Should.NotBeNull(view, "view");
            if (view.Subviews == null)
                return;
            foreach (UIView subview in view.Subviews)
                subview.RemoveFromSuperviewEx();
        }

        public static void SetHasState([NotNull]UIViewController controller, bool hasState)
        {
            if (hasState)
            {
                ServiceProvider.AttachedValueProvider.Clear(controller, NoStateKey);
                controller.InititalizeRestorationIdentifier();
            }
            else
            {
                ServiceProvider.AttachedValueProvider.SetValue(controller, NoStateKey, NoStateKey);
                controller.RestorationIdentifier = null;
            }
        }

        public static bool GetHasState([NotNull] UIViewController controller)
        {
            return !ServiceProvider.AttachedValueProvider.Contains(controller, NoStateKey);
        }

        public static void InititalizeRestorationIdentifier([NotNull] this UIView view, bool checkRestoreMethodOverload = true)
        {
            Should.NotBeNull(view, "view");
            if (string.IsNullOrEmpty(view.RestorationIdentifier))
            {
                var identifier = GenerateRestorationIdentifier(view, checkRestoreMethodOverload);
                if (identifier != null)
                    view.RestorationIdentifier = identifier;
            }
        }

        public static void InititalizeRestorationIdentifier([NotNull] this UIViewController controller, bool checkRestoreMethodOverload = true)
        {
            Should.NotBeNull(controller, "controller");
            if (string.IsNullOrEmpty(controller.RestorationIdentifier) && GetHasState(controller))
            {
                var identifier = GenerateRestorationIdentifier(controller, checkRestoreMethodOverload);
                if (identifier != null)
                    controller.RestorationIdentifier = identifier;
            }
        }

        [CanBeNull]
        public static string GenerateRestorationIdentifier(object item, bool checkRestoreMethodOverload)
        {
            Type type = item.GetType();
            int value;
            lock (TypeToCounters)
            {
                if (!TypeToCounters.TryGetValue(type, out value))
                {
                    if (typeof(UIViewController).IsAssignableFrom(type) || typeof(UIView).IsAssignableFrom(type))
                    {
                        var method = type.GetMethod("EncodeRestorableState", BindingFlags.Public | BindingFlags.Instance,
                            null, CoderParameters, Empty.Array<ParameterModifier>());
                        if (method != null && (method.DeclaringType == typeof(UIViewController) || method.DeclaringType == typeof(UIView)))
                        {
                            value = int.MinValue;
                            TypeToCounters[type] = value;
                        }
                    }
                }
                if (checkRestoreMethodOverload && value < 0)
                {
                    Tracer.Warn("The item '{0}' not support the preservation of the state since it does not have overloaded methods (EncodeRestorableState, DecodeRestorableState).", type.Name);
                    return null;
                }
                TypeToCounters[type] = value + 1;
            }
            return type.AssemblyQualifiedName + "~" + value.ToString(CultureInfo.InvariantCulture);
        }

        public static Type GetTypeFromRestorationIdentifier(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;
            var index = id.IndexOf('~');
            if (index <= 0)
                return null;
            var typeName = id.Substring(0, index);
            if (string.IsNullOrEmpty(typeName))
                return null;
            return Type.GetType(typeName, false);
        }

        public static void ClearBindings<T>([CanBeNull]this T[] items, bool clearDataContext, bool clearAttachedValues)
            where T : class, INativeObject
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Length; i++)
                items[i].ClearBindings(clearDataContext, clearAttachedValues);
        }

        internal static IMvvmViewControllerMediator GetOrCreateMediator(this UIViewController controller, ref IMvvmViewControllerMediator mediator)
        {
            if (mediator == null)
                Interlocked.CompareExchange(ref mediator, MvvmViewControllerMediatorFactory(controller, DataContext.Empty), null);
            return mediator;
        }

        internal static WeakReference CreateWeakReference(object item)
        {
            var obj = item as NSObject;
            if (obj == null)
                return new WeakReference(item, true);
            return AttachedValueProvider.GetNativeObjectWeakReference(obj);
        }

        [CanBeNull]
        internal static T FindParent<T>([CanBeNull] this INativeObject obj)
            where T : class
        {
            if (obj == null)
                return null;
            object item = BindingServiceProvider.VisualTreeManager.FindParent(obj);
            while (item != null)
            {
                var result = item as T;
                if (result != null)
                    return result;
                item = BindingServiceProvider.VisualTreeManager.FindParent(item);
            }
            return null;
        }

        internal static object SelectTemplateWithContext(this IDataTemplateSelector selector,
            [CanBeNull] object item, [NotNull] object container)
        {
            object template = selector.SelectTemplate(item, container);
            if (template != null)
            {
                template.SetDataContext(item);
                if (!(template is UIView) &&
                    template.GetBindingMemberValue(AttachedMembers.Object.Parent, container) == null)
                    template.SetBindingMemberValue(AttachedMembers.Object.Parent, container);
            }
            return template;
        }

        internal static UIView GetRootView(this UIView uiView)
        {
            UIView root = null;
            while (uiView != null)
            {
                root = uiView;
                uiView = uiView.Superview;
            }
            return root;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            Version result;
            Version.TryParse(UIDevice.CurrentDevice.SystemVersion, out result);
            return new PlatformInfo(PlatformType.iOS, result);
        }

        internal static bool IsSerializable(this Type type)
        {
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
        }

        internal static NSIndexPath[] CreateNSIndexPathArray(int startingPosition, int count)
        {
            var newIndexPaths = new NSIndexPath[count];
            for (int i = 0; i < count; i++)
                newIndexPaths[i] = NSIndexPath.FromRowSection(i + startingPosition, 0);
            return newIndexPaths;
        }

        private static void SetParent(object[] items, object parent)
        {
            if (items == null)
                return;
            for (int index = 0; index < items.Length; index++)
                items[index].SetBindingMemberValue(AttachedMembers.Object.Parent, parent);
        }

        private static void RaiseParentChanged(UIView[] items)
        {
            if (items == null)
                return;
            for (int index = 0; index < items.Length; index++)
                ParentObserver.Raise(items[index], true);
        }

        private static void AddButtonOS7([NotNull] this UIActionSheet actionSheet, string title, string binding, IList<object> sources)
        {
            var id = Guid.NewGuid().ToString("N");
            actionSheet.AddButton(id);
            var subviews = actionSheet.Subviews;
            UIButton button = null;
            if (subviews != null)
                button = subviews.OfType<UIButton>().FirstOrDefault(view => view.CurrentTitle == id);
            if (button == null)
            {
                actionSheet.AddButtonOS8(title, binding, sources);
                return;
            }
            button.SetTitle(title, button.State);
            ParentObserver.GetOrAdd(button).Parent = actionSheet;
            if (!string.IsNullOrEmpty(binding))
                button.SetBindings(binding, sources);
        }

        private static void AddButtonOS8([NotNull] this UIActionSheet actionSheet, string title, string binding, IList<object> sources)
        {
            var index = actionSheet.AddButton(title);
            var buttonClosure = new ActionSheetButtonClosure { Index = index };
            actionSheet.Clicked += buttonClosure.OnClick;
            buttonClosure.SetBindingMemberValue(AttachedMembers.Object.Parent, actionSheet);
            if (!string.IsNullOrEmpty(binding))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(buttonClosure, binding, sources);
        }

        #endregion
    }
}
