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
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using JetBrains.Annotations;
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

        [XmlIgnore, NonSerialized]
        private bool _isNotificationsDirty;

        [XmlIgnore, NonSerialized]
        private int _suspendCount;

        [XmlIgnore, NonSerialized]
        private WeakReference _ref;

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
        [field: XmlIgnore, NonSerialized]
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
            OnPropertyChanged(new PropertyChangedEventArgs(propName ?? string.Empty), executionMode);
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        /// <param name="expression">Specified expression with property.</param>
        [NotifyPropertyChangedInvocator("expression")]
        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            OnPropertyChanged(expression, PropertyChangeExecutionMode);
        }

        /// <summary>
        ///     Calls the event for the specified property.
        /// </summary>
        /// <param name="expression">Specified expression with property.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        [NotifyPropertyChangedInvocator("expression")]
        protected void OnPropertyChanged<T>(Expression<Func<T>> expression, ExecutionMode executionMode)
        {
            Should.NotBeNull(expression, "expression");
            OnPropertyChanged(expression.GetMemberInfo().Name, executionMode);
        }

        /// <summary>
        ///     Sets a property with calling property change event.
        /// </summary>
        /// <typeparam name="T">The type of property.</typeparam>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="newValue">The new property value.</param>
        /// <param name="propName">Specified expression with property.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        [NotifyPropertyChangedInvocator("propName")]
        protected void SetProperty<T>(ref T propertyValue, T newValue, [CallerMemberName] string propName = "",
            ExecutionMode? executionMode = null)
        {
            if (Equals(propertyValue, newValue)) return;
            propertyValue = newValue;
            if (executionMode == null)
                executionMode = PropertyChangeExecutionMode;
            OnPropertyChanged(propName, executionMode.Value);
        }

        /// <summary>
        ///     Sets a property with calling property change event.
        /// </summary>
        /// <typeparam name="T">The type of property.</typeparam>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="newValue">The new property value.</param>
        /// <param name="expression">Specified expression with property.</param>
        /// <param name="executionMode">
        ///     Specifies the execution mode for raise property changed event.
        /// </param>
        [NotifyPropertyChangedInvocator("expression")]
        protected void SetProperty<T>(ref T propertyValue, T newValue, Expression<Func<T>> expression,
            ExecutionMode? executionMode = null)
        {
            if (Equals(propertyValue, newValue)) return;
            propertyValue = newValue;
            if (executionMode == null)
                executionMode = PropertyChangeExecutionMode;
            OnPropertyChanged(expression, executionMode.Value);
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
        /// Occurs on end suspend notifications.
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
                OnPropertyChanged(string.Empty);
            }
            OnPropertyChanged("IsNotificationsSuspended");
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
                OnPropertyChanged("IsNotificationsSuspended");
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
                    _ref = ServiceProvider.WeakReferenceFactory(this, true);
                return _ref;
            }
        }

        #endregion
    }
}