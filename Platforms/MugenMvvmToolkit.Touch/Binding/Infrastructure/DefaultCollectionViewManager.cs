#region Copyright
// ****************************************************************************
// <copyright file="DefaultCollectionViewManager.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.UIKit;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.MonoTouch.Dialog;

namespace MugenMvvmToolkit.Binding.Infrastructure
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

        #region Implementation of ICollectionViewManager

        /// <summary>
        ///     Inserts an item to the specified index.
        /// </summary>
        public void Insert(object view, int index, object item)
        {
            var section = view as Section;
            if (section != null)
            {
                section.Insert(index, (Element)item);
                ((Element)item).RaiseParentChanged();
                return;
            }

            var rootElement = view as RootElement;
            if (rootElement != null)
            {
                rootElement.Insert(index, (Section)item);
                ((Section)item).RaiseParentChanged();
                return;
            }

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

        /// <summary>
        ///     Removes an item.
        /// </summary>
        public void RemoveAt(object view, int index)
        {
            var section = view as Section;
            if (section != null)
            {
                var element = section[index];
                section.Remove(index);
                element.ClearBindingsHierarchically(true, true, false);
                return;
            }

            var rootElement = view as RootElement;
            if (rootElement != null)
            {
                var element = rootElement[index];
                rootElement.RemoveAt(index);
                element.ClearBindingsHierarchically(true, true, false);
                return;
            }

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

        /// <summary>
        ///     Removes all items.
        /// </summary>
        public void Clear(object view)
        {
            var section = view as Section;
            if (section != null)
            {
                var elements = section.OfType<Element>().ToArray();
                section.Clear();
                foreach (var element in elements)
                    element.ClearBindingsHierarchically(true, true, false);
                return;
            }

            var rootElement = view as RootElement;
            if (rootElement != null)
            {
                var elements = rootElement.ToArray();
                rootElement.Clear();
                foreach (var element in elements)
                    element.ClearBindingsHierarchically(true, true, false);
                return;
            }

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
            if (pair.Key != null)
                pair.Key.RemoveFromParentViewController();
            if (pair.Value != null)
                pair.Value.ClearBindingsHierarchically(true, true, false);
        }

        #endregion
    }
}