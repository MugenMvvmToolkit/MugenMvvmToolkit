#region Copyright
// ****************************************************************************
// <copyright file="AttachedValueProvider.cs">
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
#if !PCL_Silverlight
using System;
using System.Runtime.CompilerServices;
using MugenMvvmToolkit.Collections;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
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

        private sealed class AttachedValueDictionary : LightDictionaryBase<string, object>
        {
            #region Constructors

            public AttachedValueDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,object>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(string x, string y)
            {
                return string.Equals(x, y, StringComparison.Ordinal);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

#if WINDOWSCOMMON || NETFX_CORE
        //NOTE ConditionalWeakTable incorrectly tracks WinRT objects https://connect.microsoft.com/VisualStudio/feedback/details/930200/conditionalweaktable-incorrectly-tracks-winrt-objects
        private static readonly DependencyProperty AttachedValueDictionaryProperty = DependencyProperty.RegisterAttached(
            "AttachedValueDictionary", typeof(AttachedValueDictionary), typeof(AttachedValueProvider), new PropertyMetadata(default(AttachedValueDictionary)));
#endif

        private static readonly ConditionalWeakTable<object, AttachedValueDictionary>.CreateValueCallback
            CreateDictionaryDelegate = o => new AttachedValueDictionary();

        private readonly ConditionalWeakTable<object, AttachedValueDictionary> _internalDictionary =
            new ConditionalWeakTable<object, AttachedValueDictionary>();

        #endregion

        #region Overrides of AttachedValueProviderBase<WeakKey,AttachedValueDictionary>

        /// <summary>
        ///     Clears all attached properties in the specified item.
        /// </summary>
        protected override bool ClearInternal(object item)
        {
#if WINDOWSCOMMON || NETFX_CORE
            var dependencyObject = item as DependencyObject;
            if (dependencyObject != null)
            {
                dependencyObject.ClearValue(AttachedValueDictionaryProperty);
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
#if WINDOWSCOMMON || NETFX_CORE
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
