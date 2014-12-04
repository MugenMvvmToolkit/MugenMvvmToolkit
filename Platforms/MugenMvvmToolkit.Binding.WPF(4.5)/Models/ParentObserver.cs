#region Copyright
// ****************************************************************************
// <copyright file="ParentObserver.cs">
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
using System;
using JetBrains.Annotations;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the weak parent observer.
    /// </summary>
    public sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private readonly WeakReference _view;
        private WeakReference _parent;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(FrameworkElement view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(FindParent(view), Empty.WeakReference, false);
            RoutedEventHandler handler = OnChanged;
            view.Loaded += handler;
            view.Unloaded += handler;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the source element.
        /// </summary>
        [CanBeNull]
        public FrameworkElement Source
        {
            get { return (FrameworkElement)_view.Target; }
        }

        /// <summary>
        ///     Gets or sets the parent of current element.
        /// </summary>
        [CanBeNull]
        public DependencyObject Parent
        {
            get { return _parent.Target as DependencyObject; }
            set
            {
                _isAttached = true;
                SetParent(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets or adds an instance of <see cref="ParentObserver" />.
        /// </summary>
        public static ParentObserver GetOrAdd(FrameworkElement element)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(element, "#ParentListener", (frameworkElement, o) => new ParentObserver(frameworkElement), null);
        }

        private void OnChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var source = GetSource();
            if (source == null)
                return;
            if (!_isAttached)
                SetParent(FindParent(source));
        }

        private void SetParent(DependencyObject value)
        {
            var source = GetSource();
            if (source == null)
                return;
            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
            Raise(source, EventArgs.Empty);
        }

        private FrameworkElement GetSource()
        {
            var source = Source;
            if (source == null)
                Clear();
            return source;
        }

        internal static DependencyObject FindParent(FrameworkElement target)
        {
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), "PlacementTarget", false, false);
            if (member != null)
            {
                object value = member.GetValue(target, null);
                if (value == null)
                    return null;
                return (DependencyObject)value;
            }
            return VisualTreeHelper.GetParent(target) ?? target.Parent;
        }

        #endregion
    }
}