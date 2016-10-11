#region Copyright

// ****************************************************************************
// <copyright file="ParentObserver.cs">
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

#if WINDOWS_UWP
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
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.Binding.Models
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
        public FrameworkElement Source => (FrameworkElement)_view.Target;

        [CanBeNull]
        public object Parent
        {
            get { return _parent.Target; }
            set
            {
                if (!_isAttached)
                {
                    _isAttached = true;
                    var view = GetSource();
                    if (view != null)
                    {
                        RoutedEventHandler handler = OnChanged;
                        view.Loaded -= handler;
                        view.Unloaded -= handler;
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

        private void OnChanged(object sender, RoutedEventArgs args)
        {
            var source = GetSource();
            if (source != null)
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

        internal static object FindParent(FrameworkElement target)
        {
            object value = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(target.GetType(), "PlacementTarget", false, false)
                ?.GetValue(target, null);
            if (value != null)
                return value;
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
