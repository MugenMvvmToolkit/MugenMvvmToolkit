#region Copyright

// ****************************************************************************
// <copyright file="NotifyPropertyChangedBase.cs">
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        protected NotifyPropertyChangedBase()
        {
            _isNotificationsDirty = false;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        [field: XmlIgnore, NonSerialized, IgnoreDataMember]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Properties

        [IgnoreDataMember, XmlIgnore]
        protected bool IsNotificationsDirty
        {
            get { return _isNotificationsDirty; }
            set { _isNotificationsDirty = value; }
        }

        protected virtual IThreadManager ThreadManager => ServiceProvider.ThreadManager;

        protected virtual ExecutionMode PropertyChangeExecutionMode => ApplicationSettings.PropertyChangeExecutionMode;

        #endregion

        #region Methods

        public void InvalidateProperties(ExecutionMode? executionMode = null)
        {
            OnPropertyChanged(Empty.EmptyPropertyChangedArgs, executionMode.GetValueOrDefault(PropertyChangeExecutionMode));
        }

        [NotifyPropertyChangedInvocator("propertyName")]
        protected internal void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(propertyName, PropertyChangeExecutionMode);
        }

        [NotifyPropertyChangedInvocator("propertyName")]
        protected internal void OnPropertyChanged(string propertyName, ExecutionMode executionMode)
        {
            if (PropertyChanged != null)
                OnPropertyChanged(string.IsNullOrEmpty(propertyName) ? Empty.EmptyPropertyChangedArgs : new PropertyChangedEventArgs(propertyName), executionMode);
        }

        [NotifyPropertyChangedInvocator("propertyName")]
        protected internal void SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = "", ExecutionMode? executionMode = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                OnPropertyChanged(propertyName, executionMode.GetValueOrDefault(PropertyChangeExecutionMode));
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            OnPropertyChanged(args, PropertyChangeExecutionMode);
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args, ExecutionMode executionMode)
        {
            Should.NotBeNull(args, nameof(args));
            if (IsNotificationsSuspended)
                IsNotificationsDirty = true;
            else
                ThreadManager.Invoke(executionMode, this, args, RaisePropertyChangedDelegate);
        }

        protected virtual void OnEndSuspendNotifications(bool isDirty)
        {
        }

        protected void ClearPropertyChangedSubscribers()
        {
            PropertyChanged = null;
        }

        protected void CleanupWeakReference()
        {
            if (_ref != null)
                _ref.Target = null;
        }

        internal virtual void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
        }

        private static void RaisePropertyChanged(NotifyPropertyChangedBase @this, PropertyChangedEventArgs args)
        {
            @this.PropertyChanged?.Invoke(@this, args);
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

        public virtual bool IsNotificationsSuspended => _suspendCount != 0;

        public virtual IDisposable SuspendNotifications()
        {
            Interlocked.Increment(ref _suspendCount);
            return WeakActionToken.Create(this, @base => @base.EndSuspendNotifications());
        }

        #endregion

        #region Implementation of IHasWeakReference

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
