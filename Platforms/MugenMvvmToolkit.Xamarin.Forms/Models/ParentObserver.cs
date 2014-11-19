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
using System.ComponentModel;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using Xamarin.Forms;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the weak parent observer.
    /// </summary>
    public sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private readonly WeakReference _parent;
        private readonly WeakReference _view;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(Element view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _parent = ServiceProvider.WeakReferenceFactory(FindParent(view), true);
            view.PropertyChanged += OnPropertyChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the source element.
        /// </summary>
        [CanBeNull]
        public Element Source
        {
            get { return (Element)_view.Target; }
        }

        /// <summary>
        ///     Gets or sets the parent of current element.
        /// </summary>
        [CanBeNull]
        public object Parent
        {
            get { return _parent.Target; }
            set
            {
                if (!_isAttached)
                {
                    _isAttached = true;
                    var element = Source;
                    if (element != null)
                        element.PropertyChanged -= OnPropertyChanged;
                }
                SetParent(GetSource(), value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets or adds an instance of <see cref="ParentObserver" />.
        /// </summary>
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
            _parent.Target = value;
            Raise(source, EventArgs.Empty);
        }

        private Element GetSource()
        {
            var source = Source;
            if (source == null)
                Clear();
            return source;
        }

        private static Element FindParent(Element target)
        {
            return target.ParentView;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (!_isAttached)
                SetParent(sender, FindParent((Element)sender));
        }

        #endregion
    }
}