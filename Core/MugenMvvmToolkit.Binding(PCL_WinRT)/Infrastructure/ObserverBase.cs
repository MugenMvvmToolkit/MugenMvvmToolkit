#region Copyright
// ****************************************************************************
// <copyright file="ObserverBase.cs">
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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that allows to observe an instance of object.
    /// </summary>
    public abstract class ObserverBase : IObserver
    {
        #region Fields

        private static readonly EventInfo CollectionChangedEvent;

        private readonly bool _isBindingContext;
        private readonly IBindingPath _path;
        private readonly object _sourceReference;
        private bool _isDisposed;
        private Exception _observationException;
        private IHandler<ValueChangedEventArgs> _listener;

        #endregion

        #region Constructors

        static ObserverBase()
        {
            CollectionChangedEvent = typeof(INotifyCollectionChanged).GetEventEx("CollectionChanged", MemberFlags.Instance | MemberFlags.Public);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObserverBase" /> class.
        /// </summary>
        protected ObserverBase([NotNull] object source, [NotNull] IBindingPath path)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(path, "path");
            _path = path;
            var bindingContext = source as IBindingContext;
            if (bindingContext == null)
                _sourceReference = MvvmExtensions.GetWeakReference(source);
            else
            {
                bindingContext.DataContextChanged += OnMemberChangedImpl;
                _sourceReference = bindingContext;
                _isBindingContext = true;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the locker.
        /// </summary>
        protected object Locker
        {
            get { return _sourceReference; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected abstract void UpdateInternal();

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        protected abstract IBindingPathMembers GetPathMembersInternal();

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        ///     Updates the observer value and then raises the value changed event.
        /// </summary>
        protected void OnMemberChangedImpl(object sender, object args)
        {
            Update();
        }

        /// <summary>
        ///     Raises the ValueChanged event.
        /// </summary>
        protected virtual void RaiseValueChanged(ValueChangedEventArgs args)
        {
            try
            {
                IHandler<ValueChangedEventArgs> listener = _listener;
                if (listener != null)
                    listener.Handle(this, args);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///     Tries to add event handler using the event listener.
        /// </summary>
        protected virtual IDisposable TryAddEventHandler(object value, IBindingMemberInfo member, IEventListener eventListener, string propertyName)
        {
            if (member.MemberType == BindingMemberType.Event)
                return null;
            var disposable = member.TryObserveMember(value, eventListener);
            if (disposable != null)
                return disposable;

            if (value is IEnumerable && value is INotifyCollectionChanged)
                return BindingProvider.Instance
                    .WeakEventManager
                    .TrySubscribe(value, CollectionChangedEvent, eventListener);

            var propertyChanged = value as INotifyPropertyChanged;
            if (propertyChanged == null)
                return null;
            return BindingProvider.Instance.WeakEventManager.Subscribe(propertyChanged, propertyName, eventListener);
        }

        /// <summary>
        ///     Gets the actual source if source.
        ///     If the source is a <see cref="IBindingContext" /> this property returns the DataContext value.
        /// </summary>
        protected internal bool GetActualSource(out object source)
        {
            if (_isBindingContext)
            {
                source = ((IBindingContext)_sourceReference).DataContext;
                return true;
            }
            source = ((WeakReference)_sourceReference).Target;
            return source != null;
        }

        private bool WeakValidate()
        {
            return _observationException == null && !_isDisposed;
        }

        private void Validate()
        {
            if (_isDisposed)
                throw ExceptionManager.ObjectDisposed(GetType());
            Exception exception = _observationException;
            if (exception != null)
                throw exception;
        }

        #endregion

        #region Implementation of IObserver

        /// <summary>
        ///     Gets the path.
        /// </summary>
        public IBindingPath Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Gets the source value.
        /// </summary>
        public object Source
        {
            get
            {
                if (_isBindingContext)
                    return _sourceReference;
                return ((WeakReference)_sourceReference).Target;
            }
        }

        /// <summary>
        ///     Gets or sets the value changed listener.
        /// </summary>
        public IHandler<ValueChangedEventArgs> Listener
        {
            get { return _listener; }
            set { _listener = value; }
        }

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        public void Update()
        {
            try
            {
                lock (_sourceReference)
                    UpdateInternal();
                _observationException = null;
            }
            catch (Exception exception)
            {
                _observationException = exception;
            }
            finally
            {
                RaiseValueChanged(ValueChangedEventArgs.FalseEventArgs);
            }
        }

        /// <summary>
        ///     Determines whether the current source is valid.
        /// </summary>
        /// <param name="throwOnError">
        ///     true to throw an exception if the source is not valid; false to return false.
        /// </param>
        /// <returns>
        ///     If <c>true</c> current source is valid, otherwise <c>false</c>.
        /// </returns>
        public bool Validate(bool throwOnError)
        {
            if (throwOnError)
            {
                Validate();
                return true;
            }
            return WeakValidate();
        }

        /// <summary>
        ///     Gets the actual source object.
        /// </summary>
        public object GetActualSource(bool throwOnError)
        {
            if (throwOnError)
                Validate();
            else if (!WeakValidate())
                return null;
            if (_isBindingContext)
                return ((IBindingContext)_sourceReference).DataContext;
            return ((WeakReference)_sourceReference).Target;
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        public IBindingPathMembers GetPathMembers(bool throwOnError)
        {
            if (throwOnError)
                Validate();
            else if (!WeakValidate())
                return UnsetBindingPathMembers.Instance;
            return GetPathMembersInternal();
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
            lock (_sourceReference)
            {
                if (_isDisposed)
                    return;
                try
                {
                    var bindingContext = Source as IBindingContext;
                    if (bindingContext != null)
                        bindingContext.DataContextChanged -= OnMemberChangedImpl;
                    _listener = null;
                    OnDispose();
                }
                finally
                {
                    _isDisposed = true;
                }
            }
        }

        #endregion
    }
}