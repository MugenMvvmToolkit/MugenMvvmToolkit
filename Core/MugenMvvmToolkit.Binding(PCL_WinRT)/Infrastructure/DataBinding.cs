#region Copyright

// ****************************************************************************
// <copyright file="DataBinding.cs">
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Accessors;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Provides high-level access to the definition of a binding, which connects the properties of binding target objects
    ///     and any data source
    /// </summary>
    public class DataBinding : IDataBinding, IDataContext, ICollection<IBindingBehavior>
    {
        #region Fields

        internal bool IsAssociated;
        private readonly IBindingSourceAccessor _sourceAccessor;
        private readonly ISingleBindingSourceAccessor _targetAccessor;
        private IDataContext _lazyContext;
        private bool _isSourceUpdating;
        private bool _isTargetUpdating;
        private IBindingBehavior[] _items;
        private int _size;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataBinding" /> class.
        /// </summary>
        public DataBinding([NotNull] ISingleBindingSourceAccessor target, [NotNull] IBindingSourceAccessor source)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(source, "source");
            _targetAccessor = target;
            _sourceAccessor = source;
            _items = Empty.Array<IBindingBehavior>();
        }

        #endregion

        #region Implementation of IDataBinding

        /// <summary>
        ///     Gets the current <see cref="IDataContext" />.
        /// </summary>
        public IDataContext Context
        {
            get { return this; }
        }

        /// <summary>
        ///     Gets the binding target accessor.
        /// </summary>
        public ISingleBindingSourceAccessor TargetAccessor
        {
            get { return _targetAccessor; }
        }

        /// <summary>
        ///     Gets the binding source accessor.
        /// </summary>
        public IBindingSourceAccessor SourceAccessor
        {
            get { return _sourceAccessor; }
        }

        /// <summary>
        ///     Gets the binding behaviors.
        /// </summary>
        public ICollection<IBindingBehavior> Behaviors
        {
            get { return this; }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return ReferenceEquals(DataContext.Empty, _lazyContext); }
        }

        /// <summary>
        ///     Sends the current value back to the source.
        /// </summary>
        public virtual bool UpdateSource()
        {
            //ignoring the concurrent access, there is no need to use Interlocked or lock
            if (_isSourceUpdating)
                return false;
            try
            {
                _isSourceUpdating = true;
                if (_sourceAccessor.SetValue(_targetAccessor, this, true))
                {
                    RaiseBindingUpdated(BindingEventArgs.SourceTrueArgs);
                    return true;
                }
                RaiseBindingUpdated(BindingEventArgs.SourceFalseArgs);
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, BindingAction.UpdateSource, exception),
                    exception, BindingAction.UpdateSource);
            }
            finally
            {
                _isSourceUpdating = false;
            }
            return false;
        }

        /// <summary>
        ///     Forces a data transfer from source to target.
        /// </summary>
        public virtual bool UpdateTarget()
        {
            //ignoring the concurrent access, there is no need to use Interlocked or lock
            if (_isTargetUpdating)
                return false;
            try
            {
                _isTargetUpdating = true;
                if (_targetAccessor.SetValue(_sourceAccessor, this, true))
                {
                    RaiseBindingUpdated(BindingEventArgs.TargetTrueArgs);
                    return true;
                }
                RaiseBindingUpdated(BindingEventArgs.TargetFalseArgs);
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, BindingAction.UpdateTarget, exception), exception,
                    BindingAction.UpdateTarget);
            }
            finally
            {
                _isTargetUpdating = false;
            }
            return false;
        }

        /// <summary>
        ///     Validates the current binding and raises the BindingException event if needed.
        /// </summary>
        public virtual bool Validate()
        {
            var action = BindingAction.UpdateTarget;
            try
            {
                bool isValid = _targetAccessor.Source.Validate(true);
                action = BindingAction.UpdateSource;

                var singleSourceAccessor = _sourceAccessor as ISingleBindingSourceAccessor;
                if (singleSourceAccessor != null)
                {
                    if (isValid && !singleSourceAccessor.Source.Validate(true))
                        isValid = false;
                }
                else
                {
                    for (int index = 0; index < _sourceAccessor.Sources.Count; index++)
                    {
                        if (isValid && !_sourceAccessor.Sources[index].Validate(true))
                            isValid = false;
                    }
                }
                return isValid;
            }
            catch (Exception exception)
            {
                RaiseBindingException(
                    BindingExceptionManager.WrapBindingException(this, action, exception), exception,
                    BindingAction.UpdateTarget);
                return false;
            }
        }

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _lazyContext, DataContext.Empty) == DataContext.Empty)
                return;
            OnDispose();
            BindingServiceProvider.BindingManager.Unregister(this);
            BindingUpdated = null;
            BindingException = null;
            ((ICollection<IBindingBehavior>)this).Clear();
            _sourceAccessor.Dispose();
            _targetAccessor.Dispose();
        }

        /// <summary>
        ///     Occurs when the binding updates the values.
        /// </summary>
        public event EventHandler<IDataBinding, BindingEventArgs> BindingUpdated;

        /// <summary>
        ///     Occurs when an exception is not caught.
        /// </summary>
        public event EventHandler<IDataBinding, BindingExceptionEventArgs> BindingException;

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when behavior added.
        /// </summary>
        protected virtual void OnBehaviorAdded([NotNull] IBindingBehavior behavior)
        {
        }

        /// <summary>
        ///     Occurs when behavior removed.
        /// </summary>
        protected virtual void OnBehaviorRemoved([NotNull] IBindingBehavior behavior)
        {
        }


        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        ///     Raises the <see cref="BindingException" /> event.
        /// </summary>
        protected void RaiseBindingException(Exception exception, Exception originalException, BindingAction action)
        {
            Tracer.Error(exception.Message);
            var handler = BindingException;
            if (handler != null) handler(this, new BindingExceptionEventArgs(action, exception, originalException));
        }

        /// <summary>
        ///     Raises the <see cref="BindingUpdated" /> event.
        /// </summary>
        protected void RaiseBindingUpdated(BindingEventArgs args)
        {
            var handler = BindingUpdated;
            if (handler != null) handler(this, args);
        }

        private void InitializeContext()
        {
            if (_lazyContext == null)
            {
                Interlocked.CompareExchange(ref _lazyContext, new DataContext(), null);
                _lazyContext.AddOrUpdate(BindingConstants.Binding, this);
            }
        }

        private void CheckBehavior(IBindingBehavior newBehavior)
        {
            Should.NotBeNull(newBehavior, "newBehavior");
            if (_size == 0)
                return;
            for (int index = 0; index < _size; index++)
            {
                if (_items[index].Id == newBehavior.Id)
                    throw BindingExceptionManager.DuplicateBehavior(_items[index], newBehavior);
            }
        }

        private void EnsureCapacity(int min)
        {
            if (_items.Length >= min)
                return;
            var value = _items.Length == 0 ? 2 : _items.Length + 1;
            if (value < _size)
                throw ExceptionManager.CapacityLessThanCollection("Capacity");
            if (value == _items.Length)
                return;
            if (value > 0)
            {
                var objArray = new IBindingBehavior[value];
                if (_size > 0)
                    Array.Copy(_items, 0, objArray, 0, _size);
                _items = objArray;
            }
            else
                _items = Empty.Array<IBindingBehavior>();
        }

        private int IndexOf(IBindingBehavior item)
        {
            return Array.IndexOf(_items, item, 0, _size);
        }

        private IEnumerator<IBindingBehavior> GetBehaviorEnumerator()
        {
            return _items
                .Take(_size)
                .GetEnumerator();
        }

        #endregion

        #region Implementation of IDataContext

        /// <summary>
        ///     Gets the number of elements contained in the <see cref="IDataContext" />.
        /// </summary>
        /// <returns>
        ///     The number of elements contained in the <see cref="IDataContext" />.
        /// </returns>
        int IDataContext.Count
        {
            get
            {
                if (_lazyContext == null)
                    return 1;
                return _lazyContext.Count;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IDataContext" /> is read-only.
        /// </summary>
        /// <returns>
        ///     true if the <see cref="IDataContext" /> is read-only; otherwise, false.
        /// </returns>
        bool IDataContext.IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Adds the data constant value.
        /// </summary>
        void IDataContext.Add<T>(DataConstant<T> dataConstant, T value)
        {
            InitializeContext();
            _lazyContext.Add(dataConstant, value);
        }

        /// <summary>
        ///     Adds the data constant value or update existing.
        /// </summary>
        void IDataContext.AddOrUpdate<T>(DataConstant<T> dataConstant, T value)
        {
            InitializeContext();
            _lazyContext.AddOrUpdate(dataConstant, value);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        T IDataContext.GetData<T>(DataConstant<T> dataConstant)
        {
            if (_lazyContext == null)
            {
                if (BindingConstants.Binding.Equals(dataConstant))
                    return (T)(object)this;
                return default(T);
            }
            return _lazyContext.GetData(dataConstant);
        }

        /// <summary>
        ///     Gets the data using the specified data constant.
        /// </summary>
        bool IDataContext.TryGetData<T>(DataConstant<T> dataConstant, out T data)
        {
            if (_lazyContext == null)
            {
                if (BindingConstants.Binding.Equals(dataConstant))
                {
                    data = (T)(object)this;
                    return true;
                }
                data = default(T);
                return false;
            }
            return _lazyContext.TryGetData(dataConstant, out data);
        }

        /// <summary>
        ///     Determines whether the <see cref="IDataContext" /> contains the specified key.
        /// </summary>
        bool IDataContext.Contains(DataConstant dataConstant)
        {
            if (_lazyContext == null)
                return BindingConstants.Binding.Constant.Equals(dataConstant);
            return _lazyContext.Contains(dataConstant);
        }

        /// <summary>
        ///     Removes the data constant value.
        /// </summary>
        bool IDataContext.Remove(DataConstant dataConstant)
        {
            if (_lazyContext == null)
                return false;
            return _lazyContext.Remove(dataConstant);
        }

        /// <summary>
        ///     Updates the current context.
        /// </summary>
        void IDataContext.Merge(IDataContext context)
        {
            InitializeContext();
            _lazyContext.Merge(context);
        }

        /// <summary>
        /// Removes all values from current context.
        /// </summary>
        void IDataContext.Clear()
        {
            if (_lazyContext != null)
                _lazyContext.Clear();
        }

        /// <summary>
        ///     Creates an instance of <see cref="IList{DataConstantValue}" /> from current context.
        /// </summary>
        IList<DataConstantValue> IDataContext.ToList()
        {
            if (_lazyContext == null || _lazyContext == DataContext.Empty)
                return new List<DataConstantValue> { BindingConstants.Binding.ToValue(this) };
            return _lazyContext.ToList();
        }

        #endregion

        #region Implementation of ICollection<IBindingBehavior>

        IEnumerator<IBindingBehavior> IEnumerable<IBindingBehavior>.GetEnumerator()
        {
            return GetBehaviorEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetBehaviorEnumerator();
        }

        void ICollection<IBindingBehavior>.Add(IBindingBehavior item)
        {
            if (IsDisposed)
                return;
            CheckBehavior(item);
            if (!item.Attach(this))
                return;
            if (_size == _items.Length)
                EnsureCapacity(_size + 1);
            _items[_size++] = item;
            OnBehaviorAdded(item);
        }

        void ICollection<IBindingBehavior>.Clear()
        {
            for (int i = 0; i < _size; i++)
            {
                var behavior = _items[i];
                behavior.Detach(this);
                OnBehaviorRemoved(behavior);
            }
            _size = 0;
            _items = Empty.Array<IBindingBehavior>();
        }

        bool ICollection<IBindingBehavior>.Contains(IBindingBehavior item)
        {
            Should.NotBeNull(item, "item");
            return IndexOf(item) >= 0;
        }

        void ICollection<IBindingBehavior>.CopyTo(IBindingBehavior[] array, int arrayIndex)
        {
            Array.Copy(_items, 0, array, arrayIndex, _size);
        }

        bool ICollection<IBindingBehavior>.Remove(IBindingBehavior item)
        {
            Should.NotBeNull(item, "item");
            int index = IndexOf(item);
            if (index < 0)
                return false;
            IBindingBehavior behavior = _items[index];
            --_size;
            if (index < _size)
                Array.Copy(_items, index + 1, _items, index, _size - index);
            _items[_size] = null;
            behavior.Detach(this);
            OnBehaviorRemoved(behavior);
            return true;
        }

        int ICollection<IBindingBehavior>.Count
        {
            get { return _size; }
        }

        bool ICollection<IBindingBehavior>.IsReadOnly
        {
            get { return false; }
        }

        #endregion
    }
}