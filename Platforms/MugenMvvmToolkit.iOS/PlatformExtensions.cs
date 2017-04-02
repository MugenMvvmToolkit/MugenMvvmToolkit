#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using Foundation;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.iOS.Binding;
using MugenMvvmToolkit.iOS.Binding.Infrastructure;
using MugenMvvmToolkit.iOS.Binding.Models;
using MugenMvvmToolkit.iOS.Infrastructure;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.Models;
using ObjCRuntime;
using UIKit;

namespace MugenMvvmToolkit.iOS
{
    public static partial class PlatformExtensions
    {
        #region Fields

        private const string NavParamKey = "@~`NavParam";
        private const string NavContextKey = "@~`NavContext";
        private const string NavContextBackKey = NavContextKey + "Back";
        private const string NoStateKey = "@$Nst";

        private static readonly Dictionary<Type, int> TypeToCounters;
        private static readonly Type[] CoderParameters;

        private static IApplicationStateManager _applicationStateManager;
        private static Func<object, IDataContext, Type, object> _mediatorFactory;
        private static Func<UITableView, IDataContext, TableViewSourceBase> _tableViewSourceFactory;
        private static Func<UICollectionView, IDataContext, CollectionViewSourceBase> _collectionViewSourceFactory;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            TypeToCounters = new Dictionary<Type, int>();
            CoderParameters = new[] { typeof(NSCoder) };
            AttachedValueProviderSuppressFinalize = true;
        }

        #endregion

        #region Properties

        [NotNull]
        public static IApplicationStateManager ApplicationStateManager
        {
            get
            {
                if (_applicationStateManager == null)
                    _applicationStateManager = ServiceProvider.Get<IApplicationStateManager>();
                return _applicationStateManager;
            }
            set { _applicationStateManager = value; }
        }

        [NotNull]
        public static Func<object, IDataContext, Type, object> MediatorFactory
        {
            get { return _mediatorFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _mediatorFactory = value;
            }
        }

        [NotNull]
        public static Func<UITableView, IDataContext, TableViewSourceBase> TableViewSourceFactory
        {
            get { return _tableViewSourceFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _tableViewSourceFactory = value;
            }
        }

        [NotNull]
        public static Func<UICollectionView, IDataContext, CollectionViewSourceBase> CollectionViewSourceFactory
        {
            get { return _collectionViewSourceFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _collectionViewSourceFactory = value;
            }
        }

        [CanBeNull]
        public static INativeObjectManager NativeObjectManager { get; set; }

        public static bool AttachedValueProviderSuppressFinalize { get; set; }

        #endregion

        #region Methods

        public static void DisposeEx<T>(this T[] items)
            where T : INativeObject
        {
            if (items == null)
                return;
            for (int i = 0; i < items.Length; i++)
                items[i].DisposeEx();
        }

        public static void DisposeEx(this INativeObject nativeObject)
        {
            NativeObjectManager?.Dispose(nativeObject, null);
        }

        public static void SetInputViewEx([NotNull] this UITextField textField, UIView view)
        {
            Should.NotBeNull(textField, nameof(textField));
            textField.InputView = view;
            if (view != null)
                ParentObserver.GetOrAdd(view).Parent = textField;
        }

        public static void SetNavigationParameter([NotNull] this UIViewController controller, string value)
        {
            Should.NotBeNull(controller, nameof(controller));
            if (value == null)
                ServiceProvider.AttachedValueProvider.Clear(controller, NavParamKey);
            else
                ServiceProvider.AttachedValueProvider.SetValue(controller, NavParamKey, value);
        }

        public static string GetNavigationParameter([CanBeNull] this UIViewController controller)
        {
            if (controller == null)
                return null;
            return ServiceProvider.AttachedValueProvider.GetValue<string>(controller, NavParamKey, false);
        }

        public static void SetToolbarItemsEx([NotNull] this UIViewController controller, UIBarButtonItem[] items,
            bool? animated = null)
        {
            Should.NotBeNull(controller, nameof(controller));
            SetParent(items, controller);
            if (animated == null)
                controller.ToolbarItems = items;
            else
                controller.SetToolbarItems(items, animated.Value);
        }

        public static void SetItemsEx([NotNull] this UIToolbar toolbar, UIBarButtonItem[] items, bool? animated = null)
        {
            Should.NotBeNull(toolbar, nameof(toolbar));
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
            Should.NotBeNull(container, nameof(container));
            item.SetBindingMemberValue(AttachedMembers.Object.Parent, container);
            setAction(container, item);
        }

        public static void SetItemsEx<T, TItem>([NotNull] this T container, Action<T, TItem[]> setAction, params TItem[] items)
            where T : INativeObject
            where TItem : class
        {
            Should.NotBeNull(container, nameof(container));
            SetParent(items, container);
            setAction(container, items);
        }

        public static void RaiseParentChanged(this UIView uiView, bool recursively = true)
        {
            ParentObserver.Raise(uiView, recursively);
        }

        public static void AddEx([NotNull] this UIViewController controller, UIView view)
        {
            Should.NotBeNull(controller, nameof(controller));
            controller.Add(view);
            view.RaiseParentChanged();
        }

        public static void AddEx([NotNull] this UIView parent, UIView view)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.Add(view);
            view.RaiseParentChanged();
        }

        public static void AddSubviewEx([NotNull] this UIView parent, UIView view)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.AddSubview(view);
            view.RaiseParentChanged();
        }

        public static void AddSubviewsEx([NotNull] this UIView parent, params UIView[] subViews)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.AddSubviews(subViews);
            RaiseParentChanged(subViews);
        }

        public static void InsertSubviewEx([NotNull] this UIView parent, UIView view, int index)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.InsertSubview(view, index);
            view.RaiseParentChanged();
        }

        public static void RemoveFromSuperviewEx([NotNull] this UIView view)
        {
            Should.NotBeNull(view, nameof(view));
            view.RemoveFromSuperview();
            view.RaiseParentChanged(false);
        }

        public static void ClearSubViews(this UIView view)
        {
            Should.NotBeNull(view, nameof(view));
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

        public static void InititalizeRestorationIdentifier([CanBeNull] this UIView view, bool checkRestoreMethodOverload = true)
        {
            if (!view.IsAlive())
                return;
            if (string.IsNullOrEmpty(view.RestorationIdentifier))
            {
                var identifier = GenerateRestorationIdentifier(view, checkRestoreMethodOverload);
                if (identifier != null)
                    view.RestorationIdentifier = identifier;
            }
        }

        public static void InititalizeRestorationIdentifier([CanBeNull] this UIViewController controller, bool checkRestoreMethodOverload = true)
        {
            if (!controller.IsAlive())
                return;
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

        public static IMvvmViewControllerMediator GetOrCreateMediator(this UIViewController controller, ref IMvvmViewControllerMediator mediator)
        {
            if (mediator == null)
                Interlocked.CompareExchange(ref mediator, (IMvvmViewControllerMediator)MediatorFactory(controller, DataContext.Empty, typeof(IMvvmViewControllerMediator)), null);
            return mediator;
        }

        [CanBeNull]
        public static T FindParent<T>([CanBeNull] this INativeObject obj)
            where T : class
        {
            if (obj == null)
                return null;
            object item = BindingServiceProvider.VisualTreeManager.GetParent(obj);
            while (item != null)
            {
                var result = item as T;
                if (result != null)
                    return result;
                item = BindingServiceProvider.VisualTreeManager.GetParent(item);
            }
            return null;
        }

        public static NSIndexPath[] CreateNSIndexPathArray(int startingPosition, int count)
        {
            var newIndexPaths = new NSIndexPath[count];
            for (int i = 0; i < count; i++)
                newIndexPaths[i] = NSIndexPath.FromRowSection(i + startingPosition, 0);
            return newIndexPaths;
        }

        public static void AddButtonWithCommand([NotNull]this UIActionSheet actionSheet, string title, [NotNull] ICommand command, object parameter = null)
        {
            Should.NotBeNull(actionSheet, nameof(actionSheet));
            Should.NotBeNull(command, nameof(command));
            var index = actionSheet.AddButton(title);
            actionSheet.Clicked += (sender, args) =>
            {
                if (args.ButtonIndex == index)
                    command.Execute(parameter);
            };
        }

        public static void SetEditingStyle([NotNull]this UITableViewCell cell, UITableViewCellEditingStyle editingStyle)
        {
            Should.NotBeNull(cell, nameof(cell));
            cell.SetBindingMemberValue(AttachedMembers.UITableViewCell.EditingStyle, editingStyle);
        }

        public static void ClearBindingsRecursively([CanBeNull]this UIView view, bool clearDataContext, bool clearAttachedValues)
        {
            if (!view.IsAlive())
                return;
            var subviews = view.Subviews;
            if (subviews != null)
            {
                foreach (var subView in subviews)
                    subView.ClearBindingsRecursively(clearDataContext, clearAttachedValues);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues);
        }

        internal static void SetNavigationContext([NotNull] this UIViewController controller, IDataContext value, bool isBack)
        {
            Should.NotBeNull(controller, nameof(controller));
            ServiceProvider.AttachedValueProvider.SetValue(controller, isBack ? NavContextBackKey : NavContextKey, value);
        }

        internal static IDataContext GetNavigationContext([CanBeNull] this UIViewController controller, bool isBack, bool remove)
        {
            if (controller == null)
                return null;
            string key = isBack ? NavContextBackKey : NavContextKey;
            var dataContext = ServiceProvider.AttachedValueProvider.GetValue<IDataContext>(controller, key, false);
            if (dataContext != null && remove)
                ServiceProvider.AttachedValueProvider.Clear(controller, key);
            return dataContext;
        }

        internal static WeakReference CreateWeakReference(object item)
        {
            var obj = item as NSObject;
            if (obj == null)
                return new WeakReference(item, true);
            return AttachedValueProvider.GetNativeObjectWeakReference(obj);
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
            return new PlatformInfo(PlatformType.iOS, UIDevice.CurrentDevice.SystemVersion);
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

        #endregion
    }
}
