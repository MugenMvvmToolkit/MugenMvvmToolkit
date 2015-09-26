#region Copyright

// ****************************************************************************
// <copyright file="ParentObserver.cs">
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

#if WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Media;
#endif
using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;

#if WPF
namespace MugenMvvmToolkit.WPF.Binding.Models
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Binding.Models
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Binding.Models
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Binding.Models
#endif
{
    internal sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private readonly WeakReference _view;
        private WeakReference _parent;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(FrameworkElement view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(FindParent(view), Empty.WeakReference, false);
            RoutedEventHandler handler = OnChanged;
            view.Loaded += handler;
            view.Unloaded += handler;
        }

        #endregion

        #region Properties

        [CanBeNull]
        public FrameworkElement Source
        {
            get { return (FrameworkElement)_view.Target; }
        }

        [CanBeNull]
        public object Parent
        {
            get { return _parent.Target as DependencyObject; }
            set
            {
                if (!_isAttached)
                {
                    _isAttached = true;
                    var view = GetSource();
                    if (view != null)
                    {
                        RoutedEventHandler handler = OnChanged;
                        view.Loaded += handler;
                        view.Unloaded += handler;
                    }
                }
                SetParent(GetSource(), value);
            }
        }

        #endregion

        #region Methods

        public static ParentObserver GetOrAdd(FrameworkElement element)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(element, "#ParentListener", (frameworkElement, o) => new ParentObserver(frameworkElement), null);
        }

        private void OnChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var source = GetSource();
            if (source != null && !_isAttached)
                SetParent(source, FindParent(source));
        }

        private void SetParent(object source, object value)
        {
            if (source == null || ReferenceEquals(value, _parent.Target))
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
                if (value != null)
                    return (DependencyObject)value;
            }
#if WPF
            if (target.IsLoaded)
                return target.Parent ?? VisualTreeHelper.GetParent(target) ?? LogicalTreeHelper.GetParent(target);
            return target.Parent;
#else
            return target.Parent ?? VisualTreeHelper.GetParent(target);
#endif
        }

        #endregion
    }
}
