#region Copyright

// ****************************************************************************
// <copyright file="AttachedValueProvider.cs">
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

using System.Collections.Generic;
#if !PCL_Silverlight
using System;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Collections;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
#elif XAMARIN_FORMS
using Xamarin.Forms;
#elif TOUCH
using Foundation;
using ObjCRuntime;
using System.Runtime.InteropServices;
using MugenMvvmToolkit.Models;
#else
using System.Windows;
#endif

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the attached value provider class, that allows to attach a value to an object using path.
    /// </summary>
#if PCL_WINRT
    public sealed class AttachedValueProviderDefault : AttachedValueProviderBase
#else
    public sealed class AttachedValueProvider : AttachedValueProviderBase
#endif

    {
        #region Nested types

#if TOUCH
        /// <summary>
        ///     https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ObjCRuntimeRef/index.html#//apple_ref/c/tdef/objc_AssociationPolicy
        /// </summary>
        private enum OBJC_ASSOCIATION_POLICY
        {
            OBJC_ASSOCIATION_RETAIN = 01401
        }

        private sealed class IntPtrComparer : IEqualityComparer<IntPtr>
        {
            #region Implementation of IEqualityComparer<in IntPtr>

            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        [Register("NSObjectEx")]
        private abstract class NSObjectEx : NSObject
        {
        }

        [Register("AttachedValueHolder")]
        private sealed class AttachedValueHolder : NSObjectEx
        {
            #region Fields

            public readonly NativeObjectWeakReference WeakReference;
            private AttachedValueDictionary _dictionary;

            #endregion

            #region Constructors

            public AttachedValueHolder(NSObject target)
            {
                objc_setAssociatedObject(target.Handle, AttachedValueKeyHandle, Handle,
                    OBJC_ASSOCIATION_POLICY.OBJC_ASSOCIATION_RETAIN);
                WeakReference = new NativeObjectWeakReference(target);
            }

            #endregion

            #region Methods

            public AttachedValueDictionary GetOrCreateDictionary()
            {
                if (_dictionary == null)
                {
                    lock (this)
                    {
                        if (_dictionary == null)
                            _dictionary = new AttachedValueDictionary();
                    }
                }
                return _dictionary;
            }

            [Export("dealloc")]
            private void Dealloc()
            {
                WeakReference.IsInvalid = true;
                lock (AttachedValueHolders)
                    AttachedValueHolders.Remove(WeakReference.Handle);
            }

            #endregion
        }
#endif

        private class AttachedValueDictionary : LightDictionaryBase<string, object>
        {
            #region Constructors

            public AttachedValueDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,object>

            protected override bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

#if !WINFORMS && !PCL_WINRT && !PCL_Silverlight && !ANDROID && !TOUCH && !XAMARIN_FORMS
        //NOTE ConditionalWeakTable incorrectly tracks WinRT objects https://connect.microsoft.com/VisualStudio/feedback/details/930200/conditionalweaktable-incorrectly-tracks-winrt-objects
        private static readonly DependencyProperty AttachedValueDictionaryProperty = DependencyProperty.RegisterAttached(
            "AttachedValueDictionary", typeof(AttachedValueDictionary), typeof(AttachedValueProvider), new PropertyMetadata(default(AttachedValueDictionary)));
#endif
#if XAMARIN_FORMS
        private static readonly BindableProperty AttachedValueDictionaryProperty = BindableProperty
            .CreateAttached("AttachedValueDictionary", typeof(AttachedValueDictionary), typeof(AttachedValueProvider),
                null);
#elif TOUCH
        private static readonly IntPtr AttachedValueKeyHandle = new NSObject().Handle;
        private static readonly Dictionary<IntPtr, AttachedValueHolder> AttachedValueHolders = new Dictionary<IntPtr, AttachedValueHolder>(new IntPtrComparer());
#endif
        private static readonly ConditionalWeakTable<object, AttachedValueDictionary>.CreateValueCallback
            CreateDictionaryDelegate = o => new AttachedValueDictionary();

        private readonly ConditionalWeakTable<object, AttachedValueDictionary> _internalDictionary =
            new ConditionalWeakTable<object, AttachedValueDictionary>();

        #endregion

        #region Methods

#if TOUCH
        /// <summary>
        ///     https://developer.apple.com/library/mac/documentation/Cocoa/Reference/ObjCRuntimeRef/index.html#//apple_ref/c/func/objc_setAssociatedObject
        /// </summary>
        [DllImport(Constants.ObjectiveCLibrary)]
        private static extern void objc_setAssociatedObject(IntPtr target, IntPtr key, IntPtr value,
            OBJC_ASSOCIATION_POLICY policy);

        internal static WeakReference GetNativeObjectWeakReference(NSObject nsObject)
        {
            var handle = nsObject.Handle;
            if (handle == IntPtr.Zero)
                return Empty.WeakReference;
            lock (AttachedValueHolders)
            {
                AttachedValueHolder value;
                if (!AttachedValueHolders.TryGetValue(handle, out value))
                {
                    value = new AttachedValueHolder(nsObject);
                    AttachedValueHolders[handle] = value;
                }
                return value.WeakReference;
            }
        }
#endif
        #endregion

        #region Overrides of AttachedValueProviderBase<WeakKey,AttachedValueDictionary>

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        protected override bool ClearInternal(object item)
        {
#if TOUCH
            var nsObject = item as NSObject;
            if (nsObject != null)
            {
                var handle = nsObject.Handle;
                if (handle == IntPtr.Zero)
                    return false;
                objc_setAssociatedObject(handle, AttachedValueKeyHandle, IntPtr.Zero, OBJC_ASSOCIATION_POLICY.OBJC_ASSOCIATION_RETAIN);

                AttachedValueHolder value;
                lock (AttachedValueHolders)
                {
                    if (AttachedValueHolders.TryGetValue(handle, out value))
                        AttachedValueHolders.Remove(handle);
                }
                if (value == null)
                    return false;
                value.Dispose();
                return true;
            }
#endif

#if !WINFORMS && !PCL_WINRT && !PCL_Silverlight && !ANDROID && !TOUCH && !XAMARIN_FORMS
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                dependencyObject.ClearValue(AttachedValueDictionaryProperty);
                return true;
            }
#endif
#if XAMARIN_FORMS
            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                bindableObject.ClearValue(AttachedValueDictionaryProperty);
                return true;
            }
#endif
            return _internalDictionary.Remove(item);
        }

        /// <summary>
        ///     Gets or adds the attached values container.
        /// </summary>
        protected override LightDictionaryBase<string, object> GetOrAddAttachedDictionary(object item, bool addNew)
        {
#if TOUCH
            var nsObject = item as NSObject;
            if (nsObject != null)
            {
                var handle = nsObject.Handle;
                if (handle == IntPtr.Zero)
                {
                    if (addNew)
                    {
                        Tracer.Error("The object {0} is disposed the attached values cannot be obtained", item);
                        return new AttachedValueDictionary();
                    }
                    return null;
                }
                AttachedValueHolder holder;
                lock (AttachedValueHolders)
                {
                    if (!AttachedValueHolders.TryGetValue(handle, out holder))
                    {
                        if (!addNew)
                            return null;
                        holder = new AttachedValueHolder(nsObject);
                        AttachedValueHolders[handle] = holder;
                    }
                }
                return holder.GetOrCreateDictionary();
            }
#endif

#if !WINFORMS && !PCL_WINRT && !PCL_Silverlight && !ANDROID && !TOUCH && !XAMARIN_FORMS
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                var dict = (AttachedValueDictionary)dependencyObject.GetValue(AttachedValueDictionaryProperty);
                if (dict == null && addNew)
                {
                    dict = new AttachedValueDictionary();
                    dependencyObject.SetValue(AttachedValueDictionaryProperty, dict);
                }
                return dict;
            }
#endif
#if XAMARIN_FORMS
            var bindableObject = item as BindableObject;
            if (bindableObject != null)
            {
                var dict = (AttachedValueDictionary)bindableObject.GetValue(AttachedValueDictionaryProperty);
                if (dict == null && addNew)
                {
                    dict = new AttachedValueDictionary();
                    bindableObject.SetValue(AttachedValueDictionaryProperty, dict);
                }
                return dict;
            }
#endif
            if (addNew)
                return _internalDictionary.GetValue(item, CreateDictionaryDelegate);
            AttachedValueDictionary value;
            _internalDictionary.TryGetValue(item, out value);
            return value;
        }

        #endregion
    }
}
#endif
