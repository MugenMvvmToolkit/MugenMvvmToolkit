using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Members;
using ObjCRuntime;
using UIKit;

namespace MugenMvvm.Ios.Extensions
{
    public static partial class IosMugenExtensions
    {
        #region Methods

        public static void Encode(this NSCoder coder, string? value, string key)
        {
            Should.NotBeNull(coder, nameof(coder));
            Should.NotBeNull(key, nameof(key));
            if (value == null)
                return;
            var s = new NSString(value);
            coder.Encode(s, key);
            s.Dispose();
        }

        public static string? DecodeString(this NSCoder coder, string key)
        {
            Should.NotBeNull(coder, nameof(coder));
            Should.NotBeNull(key, nameof(key));
            string? result = null;
            var value = coder.DecodeObject(key);
            if (value is NSString st)
                result = st.ToString();
            value?.Dispose();
            return result;
        }

        public static void SetItemsNotifyParent<T, TItem>(this T container, Action<T, TItem[]> setAction, TItem[] items)
            where T : class, INativeObject
            where TItem : class
        {
            Should.NotBeNull(container, nameof(container));
            SetParent(items, container);
            setAction(container, items);
        }

        public static void AddSubviewNotifyParent(this UIView parent, UIView view)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.AddSubview(view);
            view.RaiseParentChanged();
        }

        public static void InsertSubviewNotifyParent(this UIView parent, UIView view, int index)
        {
            Should.NotBeNull(parent, nameof(parent));
            parent.InsertSubview(view, index);
            view.RaiseParentChanged();
        }

        public static void RemoveFromSuperviewNotifyParent(this UIView? view)
        {
            if (view.IsAlive())
            {
                view.RemoveFromSuperview();
                view.RaiseParentChanged(false);
            }
        }

        public static void ClearSubViews(this UIView? view)
        {
            if (!view.IsAlive())
                return;
            var subviews = view.Subviews;
            if (subviews == null)
                return;
            foreach (UIView subview in subviews)
                subview.RemoveFromSuperviewNotifyParent();
        }

        public static void RaiseParentChanged(this UIView view, bool recursively = true)
        {
            if (!view.IsAlive())
                return;
            if (recursively)
            {
                var subviews = view.Subviews;
                if (subviews != null)
                {
                    for (var index = 0; index < subviews.Length; index++)
                        subviews[index].RaiseParentChanged(recursively);
                }
            }

            BindableMembers.For<object>().ParentNative().TryRaise(view);
        }

        public static void ClearBindings(this UIView? view, bool clearAttachedValues, bool disposeView, bool recursively = true)
        {
            if (!view.IsAlive())
                return;
            if (recursively)
            {
                var subviews = view.Subviews;
                if (subviews != null)
                {
                    for (var index = 0; index < subviews.Length; index++)
                        subviews[index].ClearBindings(clearAttachedValues, recursively);
                }
            }

            BindingMugenExtensions.ClearBindings(view, clearAttachedValues);
            if (disposeView)
                view.Dispose();
        }

        internal static bool IsAlive([NotNullWhen(true)] this INativeObject? item) => item != null && item.Handle != IntPtr.Zero;

        private static void SetParent(object[]? items, object? parent)
        {
            if (items == null)
                return;
            for (var index = 0; index < items.Length; index++)
                items[index].BindableMembers().SetParent(parent);
        }

        #endregion
    }
}