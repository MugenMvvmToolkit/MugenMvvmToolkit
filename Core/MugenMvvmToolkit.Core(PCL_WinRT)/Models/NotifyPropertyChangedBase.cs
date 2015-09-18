#region Copyright

// ****************************************************************************
// <copyright file="NotifyPropertyChangedBase.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the base class which adds support <see cref="INotifyPropertyChanged" />.
    /// </summary>
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    public abstract class NotifyPropertyChangedBase : ISuspendNotifications, IHasWeakReference
    {
        #region Fields

        private static readonly Action<NotifyPropertyChangedBase, PropertyChangedEventArgs> RaisePropertyChangedDelegate;

        [XmlIgnore, NonSerialized, IgnoreDataMember]
        internal LightDictionaryBase<string, object> AttachedValues;

        [XmlIgnore, NonSerialized, IgnoreDataMember]
        private WeakReference _ref;

        [XmlIgnore, NonSerialized, IgnoreDataMember]
        private bool _isNotificationsDirty;

        [XmlIgnore, NonSerialized, IgnoreDataMember]
        private int _suspendCount;

        #endregion

        #region Constructors

        static NotifyPropertyChangedBase()
        {
            RaisePropertyChangedDelegate = RaisePropertyChanged;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotifyPropertyChangedBase" /> class.
        /// </summary>
        protected NotifyPropertyChangedBase()
        {
            _isNotificationsDirty = false;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        /// <summary>
        ///     Occurs when a property value changes.
        /// </summary>
        [field: XmlIgnore, NonSerialized, IgnoreDataMember]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets a value indicating whether change to the collection is made when its notifications are suspended.
        /// </summary>
        [IgnoreDataMember, XmlIgnore]
        protected bool IsNotificationsDirty
        {
            get { return _isNotificationsDirty; }
            set { _isNotificationsDirty = value; }
        }

        /// <summary>
        ///     Gets the current <see cref="IThreadManager" />.
        /// </summary>
        protected virtual IThreadManager ThreadManager
        {
            get { return ServiceProvider.ThreadManager; }
        }

        /// <summary>
        ///     Specifies the execution mode for raise property change event.
        /// </summary>
        protected virtual ExecutionMode PropertyChangeExecutionMode
        {
            get { return ApplicationSettings.PropertyChangeExecutionMode; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Indicates that all properties on the object have changed
        /// </summary>
        public void InvalidateProperties(ExecutionMode? executionMode = null)
        {
            OnPropertyChanged(Empty.EmptyPropertyChangedArgs,
                executionMode.GetValueOrDefault(PropertyChangeExecutionMode));
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        [NotifyPropertyChangedInvocator("propName")]
        protected internal void OnPropertyChanged([CallerMemberName] string propName = "")
        {
            OnPropertyChanged(propName, PropertyChangeExecutionMode);
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        /// <param name="propName">Specified property name.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        [NotifyPropertyChangedInvocator("propName")]
        protected internal void OnPropertyChanged(string propName, ExecutionMode executionMode)
        {
            if (PropertyChanged != null)
                OnPropertyChanged(string.IsNullOrEmpty(propName) ? Empty.EmptyPropertyChangedArgs : new PropertyChangedEventArgs(propName), executionMode);
        }

        /// <summary>
        ///     Sets a property with calling property change event.
        /// </summary>
        /// <typeparam name="T">The type of property.</typeparam>
        /// <param name="field">The property field.</param>
        /// <param name="newValue">The new property value.</param>
        /// <param name="propName">Specified expression with property.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        [NotifyPropertyChangedInvocator("propName")]
        protected internal void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propName = "",
            ExecutionMode? executionMode = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propName, executionMode.GetValueOrDefault(PropertyChangeExecutionMode));
            }
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        /// <param name="args">The specified property args.</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args, PropertyChangeExecutionMode);
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        /// <param name="args">The specified property args.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args, ExecutionMode executionMode)
        {
            Should.NotBeNull(args, "args");
            if (IsNotificationsSuspended)
                IsNotificationsDirty = true;
            else
                ThreadManager.Invoke(executionMode, this, args, RaisePropertyChangedDelegate);
        }

        /// <summary>
        ///     Occurs on end suspend notifications.
        /// </summary>
        protected virtual void OnEndSuspendNotifications(bool isDirty)
        {
        }

        /// <summary>
        ///     Clears all <see cref="PropertyChanged" /> subscribers.
        /// </summary>
        protected void ClearPropertyChangedSubscribers()
        {
            PropertyChanged = null;
        }

        internal virtual void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
        }

        private static void RaisePropertyChanged(NotifyPropertyChangedBase @this, PropertyChangedEventArgs args)
        {
            var handler = @this.PropertyChanged;
            if (handler != null)
                handler.Invoke(@this, args);
            @this.OnPropertyChangedInternal(args);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            OnEndSuspendNotifications(IsNotificationsDirty);
            if (IsNotificationsDirty)
            {
                IsNotificationsDirty = false;
                InvalidateProperties();
            }
            OnPropertyChanged(Empty.IsNotificationsSuspendedChangedArgs);
        }

        #endregion

        #region Implementation of ISuspendNotifications

        /// <summary>
        ///     Gets or sets a value indicating whether change notifications are suspended. <c>True</c> if notifications are
        ///     suspended, otherwise, <c>false</c>.
        /// </summary>
        public virtual bool IsNotificationsSuspended
        {
            get { return _suspendCount != 0; }
        }

        /// <summary>
        ///     Suspends the change notifications until the returned <see cref="IDisposable" /> is disposed.
        /// </summary>
        /// <returns>An instance of token.</returns>
        public virtual IDisposable SuspendNotifications()
        {
            if (Interlocked.Increment(ref _suspendCount) == 1)
                OnPropertyChanged(Empty.IsNotificationsSuspendedChangedArgs);
            return WeakActionToken.Create(this, @base => @base.EndSuspendNotifications());
        }

        #endregion

        #region Implementation of IHasWeakReference

        /// <summary>
        ///     Gets the <see cref="WeakReference" /> of current object.
        /// </summary>
        WeakReference IHasWeakReference.WeakReference
        {
            get
            {
                if (_ref == null)
                    Interlocked.CompareExchange(ref _ref, ServiceProvider.WeakReferenceFactory(this), null);
                return _ref;
            }
        }

        #endregion
    }
}