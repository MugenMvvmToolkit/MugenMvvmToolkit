#region Copyright

// ****************************************************************************
// <copyright file="ParentObserver.cs">
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
using System.ComponentModel;
using MugenMvvmToolkit.Binding.Infrastructure;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Xamarin.Forms.Binding.Models
{
    internal sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private WeakReference _parent;
        private readonly WeakReference _view;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(Element view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view);
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(FindParent(view), Empty.WeakReference, false);
            view.PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Properties

        public Element Source => (Element)_view.Target;

        public object Parent
        {
            get { return _parent.Target; }
            set
            {
                if (!_isAttached)
                {
                    _isAttached = true;
                    var element = GetSource();
                    if (element != null)
                        element.PropertyChanged -= OnPropertyChanged;
                }
                SetParent(GetSource(), value);
            }
        }

        #endregion

        #region Methods

        public static ParentObserver GetOrAdd(Element element)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(element, "#ParentListener", (frameworkElement, o) => new ParentObserver(frameworkElement),
                    null);
        }

        private void SetParent(object source, object value)
        {
            if (source == null)
                return;
            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, false);
            Raise(source, EventArgs.Empty);
        }

        private Element GetSource()
        {
            var source = Source;
            if (source == null)
                Clear();
            return source;
        }

        private static object FindParent(Element target)
        {
            return target.Parent;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Parent")
                SetParent(sender, FindParent((Element)sender));
        }

        #endregion
    }
}
