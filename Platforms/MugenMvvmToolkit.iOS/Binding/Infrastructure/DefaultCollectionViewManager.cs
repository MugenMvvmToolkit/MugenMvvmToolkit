#region Copyright

// ****************************************************************************
// <copyright file="DefaultCollectionViewManager.cs">
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
using System.Collections.Generic;
using MugenMvvmToolkit.iOS.Binding.Interfaces;
using UIKit;

namespace MugenMvvmToolkit.iOS.Binding.Infrastructure
{
    internal class DefaultCollectionViewManager : ICollectionViewManager
    {
        #region Fields

        public static readonly DefaultCollectionViewManager Instance;
        private const string ItemsPath = "#$#items";

        #endregion

        #region Constructors

        static DefaultCollectionViewManager()
        {
            Instance = new DefaultCollectionViewManager();
        }

        private DefaultCollectionViewManager()
        {
        }

        #endregion

        #region Properties

        public static Func<object, int, object, bool> InsertInternalHandler;

        public static Func<object, int, bool> RemoveAtInternalHandler;

        public static Func<object, bool> ClearInternalHandler;

        #endregion

        #region Implementation of ICollectionViewManager

        public void Insert(object view, int index, object item)
        {
            if (InsertInternalHandler != null && InsertInternalHandler(view, index, item))
                return;

            UIViewController controller;
            UIView parentView;
            if (!TryGetTarget(view, item, out controller, out parentView))
            {
                TraceNotSupported(view);
                return;
            }
            var ctrls = ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(parentView, ItemsPath, (uiView, o) => new List<KeyValuePair<UIViewController, UIView>>(), null);
            var itemView = item as UIView;
            if (itemView == null)
            {
                var viewController = (UIViewController)item;
                ctrls.Insert(index, new KeyValuePair<UIViewController, UIView>(viewController, viewController.View));
                controller.AddChildViewController(viewController);
                parentView.AddSubviewEx(viewController.View);
            }
            else
            {
                ctrls.Insert(index, new KeyValuePair<UIViewController, UIView>(null, itemView));
                parentView.InsertSubviewEx(itemView, index);
            }
        }

        public void RemoveAt(object view, int index)
        {
            if (RemoveAtInternalHandler != null && RemoveAtInternalHandler(view, index))
                return;

            UIViewController controller;
            UIView parentView;
            if (!TryGetTarget(view, view, out controller, out parentView))
            {
                TraceNotSupported(view);
                return;
            }
            var items = ServiceProvider
                .AttachedValueProvider
                .GetValue<List<KeyValuePair<UIViewController, UIView>>>(parentView, ItemsPath, false);
            if (items == null)
                return;
            var pair = items[index];
            items.RemoveAt(index);
            ClearItem(pair);
        }

        public void Clear(object view)
        {
            if (ClearInternalHandler != null && ClearInternalHandler(view))
                return;

            UIViewController controller;
            UIView parentView;
            if (!TryGetTarget(view, view, out controller, out parentView))
            {
                TraceNotSupported(view);
                return;
            }
            var items = ServiceProvider
                .AttachedValueProvider
                .GetValue<List<KeyValuePair<UIViewController, UIView>>>(parentView, ItemsPath, false);
            if (items == null)
                return;
            foreach (var item in items)
                ClearItem(item);
            items.Clear();
        }

        #endregion

        #region Methods

        private static bool TryGetTarget(object view, object item, out UIViewController controller, out UIView parentView)
        {
            controller = view as UIViewController;
            if (controller == null)
            {
                parentView = view as UIView;
                UIView rootView = parentView.GetRootView();
                if (rootView != null)
                    controller = rootView.NextResponder as UIViewController;
            }
            else
                parentView = controller.View;
            return (item is UIView || controller != null) && parentView != null;
        }

        private static void TraceNotSupported(object view)
        {
            Tracer.Warn("The view '{0}' is not supported by '{1}'", view, typeof(DefaultCollectionViewManager));
        }

        private static void ClearItem(KeyValuePair<UIViewController, UIView> pair)
        {
            pair.Key?.RemoveFromParentViewController();
            if (pair.Value != null)
            {
                pair.Value.ClearBindingsRecursively(true, true);
                pair.Value.DisposeEx();
            }
        }

        #endregion
    }
}
