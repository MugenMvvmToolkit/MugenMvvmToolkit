#region Copyright

// ****************************************************************************
// <copyright file="ObserverBase.cs">
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the observer that allows to observe an instance of object.
    /// </summary>
    public abstract class ObserverBase : IObserver
    {
        #region Nested types

        internal class DefaultListener : IEventListener, IHandler<ValueChangedEventArgs>
        {
            #region Fields

            protected readonly WeakReference Ref;

            #endregion

            #region Constructors

            public DefaultListener(WeakReference @ref)
            {
                Ref = @ref;
            }

            #endregion

            #region Implementation of interfaces

            void IHandler<ValueChangedEventArgs>.Handle(object sender, ValueChangedEventArgs message)
            {
            }

            bool IEventListener.IsAlive
            {
                get { return Ref.Target != null; }
            }

            bool IEventListener.IsWeak
            {
                get { return true; }
            }

            bool IEventListener.TryHandle(object sender, object message)
            {
                var target = (ObserverBase)Ref.Target;
                if (target == null)
                    return false;
                target.Update();
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const int DisposedState = 2;
        private const int UpdatingState = 1;
        private const int DefaultState = 0;

        private static readonly EventInfo CollectionChangedEvent;
        private static readonly EventInfo ValueChangedEvent;
        private static readonly Exception DisposedException;
        private static readonly ISourceValue EmptySource;
        private static readonly IDisposable EmptyDisposable;

        private readonly IBindingPath _path;

        private IBindingPathMembers _pathMembers;
        private object _source;
        private IDisposable _sourceListener;
        private Exception _observationException;
        private int _state;
        private EventHandler<IObserver, ValueChangedEventArgs> _valueChanged;

        #endregion

        #region Constructors

        static ObserverBase()
        {
            CollectionChangedEvent = typeof(INotifyCollectionChanged).GetEventEx("CollectionChanged", MemberFlags.Instance | MemberFlags.Public);
            ValueChangedEvent = typeof(ISourceValue).GetEventEx("ValueChanged", MemberFlags.Instance | MemberFlags.Public);
            DisposedException = ExceptionManager.ObjectDisposed(typeof(ObserverBase));
            EmptyDisposable = new ActionToken(() => { });
            EmptySource = new BindingResourceObject(null);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ObserverBase" /> class.
        /// </summary>
        protected ObserverBase([NotNull] object source, [NotNull] IBindingPath path)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(path, "path");
            _path = path;
            if (source is ISourceValue)
                _source = source;
            else
                _source = ServiceProvider.WeakReferenceFactory(source);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the original source object.
        /// </summary>
        protected object OriginalSource
        {
            get { return _source; }
        }

        /// <summary>
        ///     Indicates that current observer dependes on <see cref="ValueChanged" /> subscribers.
        /// </summary>
        protected abstract bool DependsOnSubscribers { get; }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        protected abstract IBindingPathMembers UpdateInternal(IBindingPathMembers oldPath, bool hasSubscribers);

        /// <summary>
        ///     Releases the current observers.
        /// </summary>
        protected abstract void ClearObserversInternal();

        /// <summary>
        ///     Creates the source event listener.
        /// </summary>
        protected virtual IEventListener CreateSourceListener()
        {
            return new DefaultListener(ToolkitExtensions.GetWeakReference(this));
        }

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
                var handler = _valueChanged;
                if (handler != null)
                    handler(this, args);
            }
            catch
            {
                ;
            }
        }

        /// <summary>
        ///     Tries to add event handler using the event listener.
        /// </summary>
        protected virtual IDisposable TryObserveMember(object value, IBindingMemberInfo member, IEventListener eventListener, string propertyName)
        {
            if (BindingMemberType.Event.EqualsWithoutNullCheck(member.MemberType))
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
        ///     If the source is an <see cref="ISourceValue" /> this method returns the value of Value property.
        /// </summary>
        protected internal object GetActualSource()
        {
            var reference = _source as WeakReference;
            if (reference == null)
            {
                var value = ((ISourceValue)_source).Value;
                var sourceValue = value as ISourceValue;
                while (sourceValue != null)
                {
                    value = sourceValue.Value;
                    sourceValue = value as ISourceValue;
                }
                return value;
            }
            return reference.Target;
        }

        private void InitializeSourceListener()
        {
            var ctx = _source as IBindingContext;
            if (ctx == null)
            {

                if (_source is ISourceValue)
                    _sourceListener = BindingServiceProvider.WeakEventManager.TrySubscribe(_source, ValueChangedEvent, CreateSourceListener());
            }
            else
            {
                _sourceListener = WeakEventManager.AddBindingContextListener(ctx, CreateSourceListener(), true);
            }
            if (_sourceListener == null)
                _sourceListener = EmptyDisposable;
        }

        private void Update(bool notify)
        {
            if (Interlocked.CompareExchange(ref _state, UpdatingState, DefaultState) != DefaultState)
                return;
            Exception ex = null;
            var hasSubscribers = _valueChanged != null;
            try
            {
                if (_sourceListener == null)
                {
                    if (hasSubscribers || !DependsOnSubscribers)
                        InitializeSourceListener();
                }
                ClearObserversInternal();
                _pathMembers = UpdateInternal(_pathMembers, hasSubscribers);
            }
            catch (Exception exception)
            {
                ex = exception;
                _pathMembers = UnsetBindingPathMembers.Instance;
            }
            finally
            {
                if (Interlocked.CompareExchange(ref _state, DefaultState, UpdatingState) == UpdatingState)
                {
                    _observationException = ex;
                    if (DependsOnSubscribers && !hasSubscribers && _valueChanged != null)
                        Update(true);
                    else if (notify)
                        RaiseValueChanged(ValueChangedEventArgs.FalseEventArgs);
                }
            }
        }

        #endregion

        #region Implementation of IObserver

        /// <summary>
        ///     Gets an indication whether the object referenced by the current <see cref="ObserverBase" /> object has
        ///     been garbage collected.
        /// </summary>
        /// <returns>
        ///     true if the object referenced by the current <see cref="ObserverBase" /> object has not been garbage
        ///     collected and is still accessible; otherwise, false.
        /// </returns>
        public bool IsAlive
        {
            get
            {
                var reference = _source as WeakReference;
                if (reference == null)
                    return ((ISourceValue)_source).IsAlive;
                return reference.Target != null;
            }
        }

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
                var reference = _source as WeakReference;
                if (reference == null)
                    return _source;
                return reference.Target;
            }
        }

        /// <summary>
        ///     Updates the current values.
        /// </summary>
        public void Update()
        {
            Update(true);
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
            if (ReferenceEquals(_pathMembers, null))
                Update();
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
            return GetActualSource();
        }

        /// <summary>
        ///     Gets the source object include the path members.
        /// </summary>
        public IBindingPathMembers GetPathMembers(bool throwOnError)
        {
            if (ReferenceEquals(_pathMembers, null))
            {
                Interlocked.CompareExchange(ref _pathMembers, UnsetBindingPathMembers.Instance, null);
                Update();
            }
            var exception = _observationException;
            if (exception == null)
                return _pathMembers;
            if (throwOnError)
                throw exception;
            return UnsetBindingPathMembers.Instance;
        }

        /// <summary>
        ///     Occurs when value changed.
        /// </summary>
        public event EventHandler<IObserver, ValueChangedEventArgs> ValueChanged
        {
            add
            {
                if (_state == DisposedState)
                    return;
                _valueChanged += value;
                if (ReferenceEquals(_sourceListener, null))
                    Update(false);
            }
            remove { _valueChanged -= value; }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            _pathMembers = UnsetBindingPathMembers.Instance;
            _valueChanged = null;
            if (_sourceListener != null)
            {
                _sourceListener.Dispose();
                _sourceListener = null;
            }
            ClearObserversInternal();
            _observationException = DisposedException;
            if (_source is WeakReference)
                _source = Empty.WeakReference;
            else
                _source = EmptySource;
            OnDispose();
        }

        #endregion

        #region Overrides of Object

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}, Member: {1}, IsValid: {2}", Path, GetPathMembers(false).LastMember, Validate(false).ToString());
        }

        #endregion
    }
}