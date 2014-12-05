#region Copyright
// ****************************************************************************
// <copyright file="ValidatorBase.cs">
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents a base class for a validator.
    /// </summary>
    public abstract class ValidatorBase : DisposableObject, IValidator
    {
        #region Fields

        /// <summary>
        ///     Gets the result that indicates that validator should clear errors.
        /// </summary>
        protected static readonly Task<IDictionary<string, IEnumerable>> EmptyResult;

        /// <summary>
        ///     Gets the result that indicates that validator should not update errors.
        /// </summary>
        protected static readonly Task<IDictionary<string, IEnumerable>> DoNothingResult;
        private static readonly IDictionary<string, ICollection<string>> EmptyMappingDictionary;
        private static readonly IDictionary<string, IEnumerable> EmptyValidationDictionary;

        private readonly Dictionary<string, IList<object>> _internalErrors;
        private readonly IEventAggregator _eventAggregator;
        private readonly HashSet<string> _validatingMembers;
        private int _validationThreadCount;
        private IValidatorContext _context;
        private PropertyChangedEventHandler _weakPropertyHandler;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorBase" /> class.
        /// </summary>
        static ValidatorBase()
        {
            ValidateOnPropertyChangedDefault = true;
#if PCL_WINRT
            EmptyMappingDictionary = new ReadOnlyDictionary<string, ICollection<string>>(new Dictionary<string, ICollection<string>>());
            EmptyValidationDictionary = new ReadOnlyDictionary<string, IEnumerable>(new Dictionary<string, IEnumerable>());
            DoNothingResult = FromResult(new ReadOnlyDictionary<string, IEnumerable>(new Dictionary<string, IEnumerable>()));
#else
            EmptyMappingDictionary = new Dictionary<string, ICollection<string>>();
            EmptyValidationDictionary = new Dictionary<string, IEnumerable>();
            DoNothingResult = FromResult(new Dictionary<string, IEnumerable>());
#endif
            EmptyResult = FromResult(null);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorBase" /> class.
        /// </summary>
        protected ValidatorBase()
        {
            _internalErrors = new Dictionary<string, IList<object>>();
            _eventAggregator = ServiceProvider.InstanceEventAggregatorFactory(this);
            _validatingMembers = new HashSet<string>();
            ValidateOnPropertyChanged = ValidateOnPropertyChangedDefault;
        }

        #endregion

        #region Implementation of IValidator

        /// <summary>
        ///     Gets a value indicating whether an attempt to add a duplicate validator to the collection will cause an exception to be thrown.
        /// </summary>
        public virtual bool AllowDuplicate
        {
            get { return false; }
        }

        /// <summary>
        ///     Gets the initialized state of the validator.
        /// </summary>
        public bool IsInitialized
        {
            get { return _context != null; }
        }

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
        public bool IsValid
        {
            get
            {
                EnsureInitialized();
                lock (_internalErrors)
                    return IsValidInternal();
            }
        }

        /// <summary>
        ///     Gets or sets the value, that indicates that the validator will be validate property on changed. Default is true.
        /// </summary>
        public bool ValidateOnPropertyChanged { get; set; }

        /// <summary>
        ///     Gets the validator context.
        /// </summary>
        public IValidatorContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        public bool CanValidate(IValidatorContext validatorContext)
        {
            Should.NotBeNull(validatorContext, "validatorContext");
            return !IsDisposed && CanValidateContext(validatorContext) && CanValidateInternal(validatorContext);
        }

        /// <summary>
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        public void Initialize(IValidatorContext context)
        {
            EnsureNotDisposed();
            Should.NotBeNull(context, "context");
            if (Interlocked.CompareExchange(ref _context, context, null) != null)
                throw ExceptionManager.ValidatorInitialized(this);
            if (!CanValidate(context))
                throw ExceptionManager.InvalidContexValidator(this);

            OnInitialized(context);


            var notifyPropertyChanged = Instance as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                InitializeWeakHandler();
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            }
            notifyPropertyChanged = context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
            if (notifyPropertyChanged != null)
            {
                InitializeWeakHandler();
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            }
            if (_weakPropertyHandler == null)
                Tracer.Warn(
                    "The type {0} doesn't implement the INotifyPropertyChanged, validator {1} cannot track errors.",
                    Instance.GetType(), GetType());
        }

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or
        ///     <see cref="F:System.String.Empty" />, to retrieve entity-level errors.
        /// </param>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        public IList<object> GetErrors(string propertyName)
        {
            EnsureInitialized();
            lock (_internalErrors)
                return GetErrorsInternal(propertyName);
        }

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
        public IDictionary<string, IList<object>> GetErrors()
        {
            EnsureInitialized();
            lock (_internalErrors)
                return GetErrorsInternal();
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        public Task ValidateAsync(string propertyName)
        {
            EnsureNotDisposed();
            EnsureInitialized();
            if (string.IsNullOrEmpty(propertyName))
                return ValidateAsync();
            if (IgnoreProperties.Contains(propertyName))
                return Empty.Task;

            List<string> properties = null;
            string singleMap = null;
            foreach (var item in PropertyMappings)
            {
                if (!item.Value.Contains(propertyName))
                    continue;
                if (singleMap == null)
                    singleMap = item.Key;
                else
                {
                    if (properties == null)
                        properties = new List<string> { singleMap };
                    properties.Add(item.Key);
                }
            }
            if (properties != null)
            {
                var tasks = new Task[properties.Count];
                for (int index = 0; index < properties.Count; index++)
                    tasks[index] = Validate(properties[index]);
                return ToolkitExtensions.WhenAll(tasks);
            }
            if (singleMap == null)
                singleMap = propertyName;
            return Validate(singleMap);
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        public Task ValidateAsync()
        {
            EnsureNotDisposed();
            EnsureInitialized();
            if (IgnoreProperties.Contains(string.Empty))
                return Empty.Task;
            return Validate(string.Empty);
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        public void ClearErrors(string propertyName)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(propertyName))
                ClearErrorsInternal();
            else
                ClearErrorsInternal(propertyName);
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        public void ClearErrors()
        {
            EnsureInitialized();
            ClearErrorsInternal();
        }

        /// <summary>
        ///     Creates a new validator that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator that is a copy of this instance.
        /// </returns>
        public IValidator Clone()
        {
            IValidator clone = CloneInternal();
            var validatorBase = clone as ValidatorBase;
            if (validatorBase != null)
                validatorBase.ValidateOnPropertyChanged = ValidateOnPropertyChanged;
            return clone;
        }

        /// <summary>
        ///     Gets a value that indicates whether the entity has validation errors.
        /// </summary>
        /// <returns>
        ///     true if the entity currently has validation errors; otherwise, false.
        /// </returns>
        public bool HasErrors
        {
            get { return !IsValid; }
        }

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to
        ///     retrieve entity-level errors.
        /// </param>
        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }

        /// <summary>
        ///     Occurs when the validation errors have changed for a property or for the entire entity.
        /// </summary>
        public virtual event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

#if NONOTIFYDATAERROR
        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var error = GetErrors(columnName).FirstOrDefault();
                if (error == null)
                    return null;
                return error.ToString();
            }
        }

        string IDataErrorInfo.Error
        {
            get
            {
                var errors = GetErrors(string.Empty);
                if (errors.Count == 0)
                    return null;
                return string.Join(Environment.NewLine, errors);
            }
        }
#endif
        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
        public bool Subscribe(ISubscriber subscriber)
        {
            return SubscribeInternal(subscriber);
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        public bool Unsubscribe(ISubscriber subscriber)
        {
            return UnsubscribeInternal(subscriber);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the value, that indicates that the validator will be validate property on changed. Default is true.
        /// </summary>
        public static bool ValidateOnPropertyChangedDefault { get; set; }

        /// <summary>
        ///     Gets the dictionary that contains all errors.
        /// </summary>
        protected Dictionary<string, IList<object>> InternalErrors
        {
            get { return _internalErrors; }
        }

        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        [NotNull]
        protected object Instance
        {
            get
            {
                EnsureInitialized();
                return Context.Instance;
            }
        }

        /// <summary>
        ///     Gets the mapping of model properties.
        /// </summary>
        [NotNull]
        protected IDictionary<string, ICollection<string>> PropertyMappings
        {
            get
            {
                if (Context == null)
                    return EmptyMappingDictionary;
                return Context.PropertyMappings;
            }
        }

        /// <summary>
        ///     Gets the list of properties that will not be validated.
        /// </summary>
        [NotNull]
        protected ICollection<string> IgnoreProperties
        {
            get
            {
                if (Context == null)
                    return Empty.Array<string>();
                return Context.IgnoreProperties;
            }
        }

        /// <summary>
        ///     Gets a value that indicates that the validator is now validated.
        /// </summary>
        protected bool IsValidating
        {
            get { return _validationThreadCount != 0; }
        }

        /// <summary>
        /// Gets the current sync object to access to dictionary.
        /// </summary>
        protected object Locker
        {
            get { return _internalErrors; }
        }

        /// <summary>
        ///     Gets the local <see cref="IEventAggregator" />.
        /// </summary>
        protected IEventAggregator LocalEventAggregator
        {
            get { return _eventAggregator; }
        }

        #endregion

        #region Virtual-abstract methods

        /// <summary>
        ///     Subscribes an instance to events.
        /// </summary>
        /// <param name="subscriber">The instance to subscribe for event publication.</param>
        protected virtual bool SubscribeInternal(ISubscriber subscriber)
        {
            return !ReferenceEquals(subscriber.Target, this) && _eventAggregator.Subscribe(subscriber);
        }

        /// <summary>
        ///     Unsubscribes the instance from all events.
        /// </summary>
        /// <param name="subscriber">The instance to unsubscribe.</param>
        protected virtual bool UnsubscribeInternal(ISubscriber subscriber)
        {
            return _eventAggregator.Unsubscribe(subscriber);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        protected virtual void Publish(object message)
        {
            Should.NotBeNull(message, "message");
            _eventAggregator.Publish(this, message);
        }

        /// <summary>
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        protected virtual void OnInitialized(IValidatorContext context)
        {
        }

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
        protected virtual IDictionary<string, IList<object>> GetErrorsInternal()
        {
            return new Dictionary<string, IList<object>>(_internalErrors);
        }

        /// <summary>
        ///     Gets the validation errors for a specified property or for the entire entity.
        /// </summary>
        /// <returns>
        ///     The validation errors for the property or entity.
        /// </returns>
        /// <param name="propertyName">
        ///     The name of the property to retrieve validation errors for; or null or <see cref="F:System.String.Empty" />, to
        ///     retrieve entity-level errors.
        /// </param>
        [NotNull]
        protected virtual IList<object> GetErrorsInternal(string propertyName)
        {
            if (_internalErrors.Count == 0)
                return Empty.Array<object>();

            if (string.IsNullOrEmpty(propertyName))
            {
                var objects = new List<object>();
                foreach (var error in _internalErrors.Values)
                    objects.AddRange(error);
                return objects;
            }

            IList<object> list;
            _internalErrors.TryGetValue(propertyName, out list);
            return list ?? Empty.Array<object>();
        }

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
        protected virtual bool IsValidInternal()
        {
            return _internalErrors.Count == 0 && !IsValidating;
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected virtual void ClearErrorsInternal(string propertyName)
        {
            UpdateErrors(propertyName, null, false);
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        protected virtual void ClearErrorsInternal()
        {
            string[] keys;
            lock (_internalErrors)
                keys = _internalErrors.Keys.ToArrayEx();
            for (int index = 0; index < keys.Length; index++)
                UpdateErrors(keys[index], null, false);
        }

        /// <summary>
        ///     Raises this object's ErrorsChangedChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has new errors.</param>
        /// <param name="isAsyncValidate">Indicates that property was async validation.</param>
        protected virtual void RaiseErrorsChanged(string propertyName, bool isAsyncValidate)
        {
            Publish(new DataErrorsChangedMessage(propertyName, isAsyncValidate));
            EventHandler<DataErrorsChangedEventArgs> handler = ErrorsChanged;
            if (handler != null)
                handler(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        internal virtual bool CanValidateContext(IValidatorContext validatorContext)
        {
            return true;
        }

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        protected virtual bool CanValidateInternal(IValidatorContext validatorContext)
        {
            return true;
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName);

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync();

        /// <summary>
        ///     Creates a new validator that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator that is a copy of this instance.
        /// </returns>
        protected virtual IValidator CloneInternal()
        {
            var iocContainer = ServiceProvider.IocContainer;
            if (iocContainer == null)
                return (IValidator)Activator.CreateInstance(GetType());
            return (IValidator)iocContainer.Get(GetType());
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates a <see cref="T:System.Threading.Tasks.Task`1" /> that's completed successfully with the specified result.
        /// </summary>
        /// <returns>
        ///     The successfully completed task.
        /// </returns>
        /// <param name="result">The result to store into the completed task.</param>
        protected static Task<IDictionary<string, IEnumerable>> FromResult(IDictionary<string, IEnumerable> result)
        {
            return ToolkitExtensions.FromResult(result);
        }

        /// <summary>
        ///     Sets errors for a property with notification.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="errors">The collection of errors.</param>
        /// <param name="isAsyncValidate">Indicates that the validation was asynchronous.</param>
        protected internal void UpdateErrors([NotNull] string propertyName, [CanBeNull] IEnumerable errors, bool isAsyncValidate)
        {
            ICollection<string> updatedProperties;
            lock (_internalErrors)
                updatedProperties = UpdateErrorsInternal(propertyName, errors);
            foreach (string name in updatedProperties)
                RaiseErrorsChanged(name, isAsyncValidate);
        }

        /// <summary>
        ///     Checks whether the member names are equal.
        /// </summary>
        /// <param name="memberName">The specified member name.</param>
        /// <param name="getMember">The expression to get member.</param>
        /// <returns>If true member names is equal, otherwise false.</returns>
        [Pure]
        protected static bool PropertyNameEqual<T>(string memberName, Expression<Func<T, object>> getMember)
        {
            return ToolkitExtensions.MemberNameEqual(memberName, getMember);
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        protected static string GetPropertyName<T>(Expression<Func<T, object>> expression)
        {
            return ToolkitExtensions.GetMemberName(expression);
        }

        /// <summary>
        ///     Notifies on errors when any property changed.
        /// </summary>
        protected void OnPropertyChangedNotifyDataError(PropertyChangedEventArgs args)
        {
            if (args == null || !ValidateOnPropertyChanged || !IsInitialized || IsDisposed)
                return;
            ValidateAsync(args.PropertyName).WithTaskExceptionHandler(this, Context.ServiceProvider as IIocContainer);
        }

        /// <summary>
        ///     Makes sure that the object is initialized.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!IsInitialized)
                throw ExceptionManager.ValidatorNotInitialized(this);
        }

        private Task Validate(string propertyName)
        {
            var validateAll = string.IsNullOrWhiteSpace(propertyName);
            if (validateAll)
                propertyName = string.Empty;
            lock (_validatingMembers)
            {
                if (!_validatingMembers.Add(propertyName))
                    return Empty.Task;
            }

            AsyncValidationMessage message = null;
            bool isAsync;
            Task<IDictionary<string, IEnumerable>> validationTask = null;
            try
            {
                validationTask = validateAll ? ValidateInternalAsync() : ValidateInternalAsync(propertyName);
                if (validationTask == null)
                    validationTask = EmptyResult;
                if (ReferenceEquals(validationTask, DoNothingResult))
                {
                    validationTask = null;
                    return Empty.Task;
                }

                isAsync = !validationTask.IsCompleted;
                if (isAsync)
                {
                    message = new AsyncValidationMessage(Guid.NewGuid(), propertyName, false);
                    Publish(message);
                    Interlocked.Increment(ref _validationThreadCount);
                    Tracer.Info("Start asynchronous validation, property name '{0}', validator '{1}'", propertyName,
                        GetType());
                }
            }
            catch
            {
                validationTask = null;
                throw;
            }
            finally
            {
                if (validationTask == null)
                {
                    lock (_validatingMembers)
                        _validatingMembers.Remove(propertyName);
                }
            }


            return validationTask.TryExecuteSynchronously(task =>
            {
                try
                {
                    if (ReferenceEquals(task.Result, DoNothingResult.Result))
                        return;

                    var properties = new HashSet<string>(StringComparer.Ordinal);
                    lock (_internalErrors)
                    {
                        //Clearing old errors
                        if (validateAll)
                        {
                            var keys = _internalErrors.Keys.ToArrayEx();
                            for (int index = 0; index < keys.Length; index++)
                                properties.AddRange(UpdateErrorsInternal(keys[index], null));
                        }
                        else
                            properties.AddRange(UpdateErrorsInternal(propertyName, null));

                        //Updating new errors
                        var result = task.Result ?? EmptyValidationDictionary;
                        foreach (var valuePair in result)
                            properties.AddRange(UpdateErrorsInternal(valuePair.Key, valuePair.Value));
                    }
                    foreach (var property in properties)
                        RaiseErrorsChanged(property, isAsync);
                }
                finally
                {
                    lock (_validatingMembers)
                        _validatingMembers.Remove(propertyName);
                    if (isAsync)
                    {
                        Interlocked.Decrement(ref _validationThreadCount);
                        if (message != null)
                            Publish(message.ToEndMessage());
                        Tracer.Info("Finish asynchronous validation, property name '{0}', validator '{1}'",
                            propertyName, GetType());
                    }
                }
            });
        }

        /// <summary>
        ///     Sets errors for a property with notification.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="validatorErrors">The collection of errors.</param>
        private ICollection<string> UpdateErrorsInternal([NotNull] string propertyName, [CanBeNull] IEnumerable validatorErrors)
        {
            Should.NotBeNull(propertyName, "propertyName");
            IList<object> errors;
            if (validatorErrors is string)
                errors = new object[] { validatorErrors };
            else
            {
                errors = validatorErrors == null
                    ? Empty.Array<object>()
                    : validatorErrors.OfType<object>().ToArray();
            }

            var hasErrors = errors.Count != 0;
            if (hasErrors && IgnoreProperties.Contains(propertyName))
                return Empty.Array<string>();

            ICollection<string> mappingProperties;
            PropertyMappings.TryGetValue(propertyName, out mappingProperties);
            if (mappingProperties == null)
            {
                if (hasErrors)
                    _internalErrors[propertyName] = errors;
                else
                    _internalErrors.Remove(propertyName);
                return new[] { propertyName };
            }

            foreach (string property in mappingProperties)
            {
                if (hasErrors)
                    _internalErrors[property] = errors;
                else
                    _internalErrors.Remove(property);
            }
            return mappingProperties;
        }

        private void InitializeWeakHandler()
        {
            if (_weakPropertyHandler == null)
                _weakPropertyHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this, (@base, o, arg3) => @base.OnPropertyChangedNotifyDataError(arg3));
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                var notifyPropertyChanged = Instance as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                    notifyPropertyChanged.PropertyChanged -= _weakPropertyHandler;
                notifyPropertyChanged = Context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
                if (notifyPropertyChanged != null)
                    notifyPropertyChanged.PropertyChanged -= _weakPropertyHandler;
                _weakPropertyHandler = null;
                ErrorsChanged = null;
                ServiceProvider.AttachedValueProvider.Clear(this);
            }
            base.OnDispose(disposing);
        }

        #endregion
    }

    /// <summary>
    ///     Represents a base class for a validator.
    /// </summary>
    public abstract class ValidatorBase<T> : ValidatorBase
    {
        #region Properties

        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        [NotNull]
        protected new T Instance
        {
            get { return (T)base.Instance; }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Checks whether the member names are equal.
        /// </summary>
        /// <param name="memberName">The specified member name.</param>
        /// <param name="getMember">The expression to get member.</param>
        /// <returns>If true member names is equal, otherwise false.</returns>
        [Pure]
        protected static bool PropertyNameEqual<TValue>(string memberName, [NotNull] Expression<Func<T, TValue>> getMember)
        {
            Should.NotBeNull(getMember, "getMember");
            return string.Equals(memberName, getMember.GetMemberInfo().Name, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        protected static string GetPropertyName<TValue>(Expression<Func<T, TValue>> expression)
        {
            return ToolkitExtensions.GetMemberName(expression);
        }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        internal override bool CanValidateContext(IValidatorContext validatorContext)
        {
            return validatorContext.Instance is T;
        }

        #endregion
    }
}