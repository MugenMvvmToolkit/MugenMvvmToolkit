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

        private ParentObserver(VisualElement view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _parent = ServiceProvider.WeakReferenceFactory(FindParent(view), true);
            view.MeasureInvalidated += OnChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the source element.
        /// </summary>
        [CanBeNull]
        public VisualElement Source
        {
            get { return (VisualElement) _view.Target; }
        }

        /// <summary>
        ///     Gets or sets the parent of current element.
        /// </summary>
        [CanBeNull]
        public VisualElement Parent
        {
            get { return _parent.Target as VisualElement; }
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
        public static ParentObserver GetOrAdd(VisualElement element)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(element, "#ParentListener", (frameworkElement, o) => new ParentObserver(frameworkElement),
                    null);
        }

        private void OnChanged(object sender, EventArgs args)
        {
            VisualElement source = GetSource();
            if (source == null)
                return;
            if (!_isAttached)
                SetParent(FindParent(source));
        }

        private void SetParent(VisualElement value)
        {
            VisualElement source = GetSource();
            if (source == null)
                return;
            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent.Target = value;
            Raise(source, EventArgs.Empty);
        }

        private VisualElement GetSource()
        {
            VisualElement source = Source;
            if (source == null)
                Clear();
            return source;
        }

        private static VisualElement FindParent(VisualElement target)
        {
            return target.ParentView;
        }

        #endregion
    }
}