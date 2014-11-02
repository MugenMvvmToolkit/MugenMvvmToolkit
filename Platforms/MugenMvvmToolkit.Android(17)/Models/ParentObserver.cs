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
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the weak parent observer.
    /// </summary>
    public sealed class ParentObserver : EventListenerList
    {
        #region Fields

        private const string Key = "!#ParentListener";
        private readonly WeakReference _view;
        private readonly WeakReference _parent;
        private bool _isAttached;

        #endregion

        #region Constructors

        private ParentObserver(View view)
        {
            _view = ServiceProvider.WeakReferenceFactory(view, true);
            _parent = ServiceProvider.WeakReferenceFactory(view.Parent, true);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the source element.
        /// </summary>
        [CanBeNull]
        public View Source
        {
            get { return (View)_view.Target; }
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
                _isAttached = true;
                SetParent(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets or adds an instance of <see cref="ParentObserver" />.
        /// </summary>
        public static ParentObserver GetOrAdd(View view)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(view, Key, (v, o) => new ParentObserver(v), null);
        }

        /// <summary>
        ///     Raises the parent changed event.
        /// </summary>
        public static void Raise(View view)
        {
            ParentObserver observer;
            if (ServiceProvider.AttachedValueProvider.TryGetValue(view, Key, out observer))
                observer.Raise();
        }

        /// <summary>
        ///     Raises the parent changed event.
        /// </summary>
        public void Raise()
        {
            var view = GetSource();
            if (view == null)
                return;
            if (_isAttached || view.Id == Android.Resource.Id.Content || ReferenceEquals(view.Parent, _parent.Target))
                return;
            _parent.Target = view.Parent;
            Raise(view, EventArgs.Empty);
        }

        private void SetParent(object value)
        {
            var view = GetSource();
            if (view == null)
                return;

            if (ReferenceEquals(value, _parent.Target))
                return;
            _parent.Target = value;
            Raise(view, EventArgs.Empty);
        }

        private View GetSource()
        {
            var source = Source;
            if (source == null)
                Clear();
            return source;
        }

        #endregion
    }
}