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
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that allows to observe an instance of object.
    /// </summary>
    public abstract class ObserverBase : IObserver
    {
        #region Fields

        private const int DisposedState = 2;
        private const int UpdatingState = 1;
        private const int DefaultState = 0;

        private static readonly EventInfo CollectionChangedEvent;
        private static readonly Exception DisposedException;

        private readonly bool _isSourceValue;
        private readonly object _sourceValue;
        private readonly IBindingPath _path;

        private int _state;
        private Exception _observationException;
        private IHandler<ValueChangedEventArgs> _listener;

        #endregion

        #region Constructors

        static ObserverBase()
        {
            CollectionChangedEvent = typeof(INotifyCollectionChanged).GetEventEx("CollectionChanged", MemberFlags.Instance | MemberFlags.Public);
            DisposedException = ExceptionManager.ObjectDisposed(typeof(ObserverBase));
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObserverBase" /> class.
        /// </summary>
        protected ObserverBase([NotNull] object source, [NotNull] IBindingPath path)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(path, "path");
            _path = path;
            var sourceValue = source as ISourceValue;
            if (sourceValue == null)
                _sourceValue = ServiceProvider.WeakReferenceFactory(source, true);
            else
            {
                _isSourceValue = true;
                _sourceValue = sourceValue;
                sourceValue.ValueChanged += OnMemberChangedImpl;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the original source object.
        /// </summary>
        protected object OriginalSource
        {
            get { return _sourceValue; }
        }

        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ObserverBase" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ObserverBase" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        protected bool IsAlive
        {
            get
            {
                if (_isSourceValue)
                    return ((ISourceValue)_sourceValue).IsAlive;
                return ((WeakReference)_sourceValue).Target != null;
            }
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
            catch
            {
            }
        }

        /// <summary>
        ///     Tries to add event handler using the event listener.
        /// </summary>
        protected virtual IDisposable TryObserveMember(object value, IBindingMemberInfo member, IEventListener eventListener, string propertyName)
        {
            if (member.MemberType == BindingMemberType.Event)
                return null;
            var disposable = member.TryObserve(value, eventListener);
            if (disposable != null)
                return disposable;

            if (value is IEnumerable && value is INotifyCollectionChanged)
                return BindingServiceProvider
                    .WeakEventManager
                    .TrySubscribe(value, CollectionChangedEvent, eventListener);

            var propertyChanged = value as INotifyPropertyChanged;
            if (propertyChanged == null)
                return null;
            return BindingServiceProvider.WeakEventManager.Subscribe(propertyChanged, propertyName, eventListener);
        }

        /// <summary>
        ///     Gets the actual source.
        ///     If the source is a <see cref="ISourceValue" /> this property returns the value of Value property.
        /// </summary>
        protected internal object GetActualSource()
        {
            if (_isSourceValue)
                return ((ISourceValue)_sourceValue).Value;
            return ((WeakReference)_sourceValue).Target;
        }

        /// <summary>
        ///     Updates the observer value and then raises the value changed event.
        /// </summary>
        private void OnMemberChangedImpl(object sender, object args)
        {
            Update();
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
                if (_isSourceValue)
                    return _sourceValue;
                return ((WeakReference)_sourceValue).Target;
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
            if (Interlocked.CompareExchange(ref _state, UpdatingState, DefaultState) != DefaultState)
                return;
            Exception ex = null;
            try
            {
                UpdateInternal();
            }
            catch (Exception exception)
            {
                ex = exception;
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _state, DefaultState, UpdatingState) == UpdatingState)
                    _observationException = ex;
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
            var exception = _observationException;
            if (exception == null)
                return true;
            if (throwOnError)
                throw exception;
            return false;
        }

        /// <summary>
        ///     Gets the actual source object.
        /// </summary>
        public object GetActualSource(bool throwOnError)
        {
            if (throwOnError)
            {
                var exception = _observationException;
                if (exception != null)
                    throw exception;
            }
            if (_isSourceValue)
                return ((ISourceValue)_sourceValue).Value;
            return ((WeakReference)_sourceValue).Target;
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        public IBindingPathMembers GetPathMembers(bool throwOnError)
        {
            var exception = _observationException;
            if (exception == null)
                return GetPathMembersInternal();
            if (throwOnError)
                throw exception;
            return UnsetBindingPathMembers.Instance;
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            _observationException = DisposedException;
            var sourceValue = _sourceValue as ISourceValue;
            if (sourceValue != null)
                sourceValue.ValueChanged -= OnMemberChangedImpl;
            _listener = null;
            OnDispose();
        }

        #endregion
    }
}