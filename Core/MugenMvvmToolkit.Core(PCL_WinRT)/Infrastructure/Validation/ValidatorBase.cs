#region Copyright

// ****************************************************************************
// <copyright file="ValidatorBase.cs">
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
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Models.Validation;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents a base class for a validator.
    /// </summary>
    public abstract class ValidatorBase : IValidator
    {
        #region Nested types

        private sealed class CancelableClosure
        {
            #region Fields

            private ValidatorBase _validator;
            private readonly bool _validateAll;
            private readonly string _propertyName;
            private readonly bool _isAsync;
            private readonly AsyncValidationMessage _message;
            private readonly CancellationTokenSource _token;

            #endregion

            #region Constructors

            public CancelableClosure(ValidatorBase validator, bool validateAll, string propertyName, bool isAsync, AsyncValidationMessage message, CancellationTokenSource token)
            {
                _validator = validator;
                _validateAll = validateAll;
                _propertyName = propertyName;
                _isAsync = isAsync;
                _message = message;
                _token = token;
            }

            #endregion

            #region Methods

            public void Callback(Task<IDictionary<string, IEnumerable>> task)
            {
                var validator = Interlocked.Exchange(ref _validator, null);
                if (validator == null)
                    return;
                bool canceled = false;
                lock (validator._runningTask)
                {
                    CancelableClosure value;
                    if (validator._runningTask.TryGetValue(_propertyName, out value) && ReferenceEquals(value, this))
                        validator._runningTask.Remove(_propertyName);
                    else
                        canceled = true;
                }
                if (canceled)
                {
                    validator.OnCanceled(_propertyName, _message);
                    _token.Cancel();
                }
                else
                {
                    validator.OnValidated(task, _validateAll, _propertyName, _isAsync, _message);
                    _token.Dispose();
                }
            }

            public void Cancel()
            {
                var validator = Interlocked.Exchange(ref _validator, null);
                if (validator == null)
                    return;
                validator.OnCanceled(_propertyName, _message);
                _token.Cancel();
            }

            #endregion
        }

        #endregion

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
        private static readonly PropertyChangedEventHandler DisposedHandler;

        private readonly Dictionary<string, IList<object>> _errors;
        //The list is used to prevent cyclic validation.
        private readonly List<string> _validatingMembers;

        private Dictionary<string, CancelableClosure> _runningTask;
        private IValidatorContext _context;
        private PropertyChangedEventHandler _weakPropertyHandler;

        #endregion

        #region Constructors

        static ValidatorBase()
        {
            ValidateOnPropertyChangedDefault = true;
#if PCL_WINRT
            EmptyMappingDictionary =
                new ReadOnlyDictionary<string, ICollection<string>>(new Dictionary<string, ICollection<string>>());
            EmptyValidationDictionary =
                new ReadOnlyDictionary<string, IEnumerable>(new Dictionary<string, IEnumerable>());
            DoNothingResult =
                FromResult(new ReadOnlyDictionary<string, IEnumerable>(new Dictionary<string, IEnumerable>()));
#else
            EmptyMappingDictionary = new Dictionary<string, ICollection<string>>();
            EmptyValidationDictionary = new Dictionary<string, IEnumerable>();
            DoNothingResult = FromResult(new Dictionary<string, IEnumerable>());
#endif
            EmptyResult = FromResult(null);
            DisposedHandler = (sender, args) => { };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorBase" /> class.
        /// </summary>
        protected ValidatorBase()
        {
            _errors = new Dictionary<string, IList<object>>(StringComparer.Ordinal);
            _validatingMembers = new List<string>();
            ValidateOnPropertyChanged = ValidateOnPropertyChangedDefault;
        }

        #endregion

        #region Implementation of IValidator

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return ReferenceEquals(_weakPropertyHandler, DisposedHandler); }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is initialized.
        /// </summary>
        public bool IsInitialized
        {
            get { return Context != null; }
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
                if (Context == null)
                    return false;
                lock (_errors)
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
        ///     Initializes the current validator using the specified <see cref="IValidatorContext" />.
        /// </summary>
        /// <param name="context">
        ///     The specified <see cref="IValidatorContext" />.
        /// </param>
        public bool Initialize(IValidatorContext context)
        {
            if (IsDisposed)
                throw ExceptionManager.ObjectDisposed(GetType());
            Should.NotBeNull(context, "context");
            if (!CanValidateContext(context) || !CanValidateInternal(context))
                return false;
            if (Interlocked.CompareExchange(ref _context, context, null) != null)
                throw ExceptionManager.ValidatorInitialized(this);

            OnInitialized(context);
            var notifyPropertyChanged = Instance as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
            {
                var vm = context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
                if (vm != null)
                {
                    InitializeWeakHandler();
                    vm.PropertyChanged += _weakPropertyHandler;
                }
            }
            else
            {
                InitializeWeakHandler();
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            }
            if (_weakPropertyHandler == null && ValidateOnPropertyChanged)
                Tracer.Warn("The type {0} doesn't implement the INotifyPropertyChanged, validator {1} cannot track errors.", Instance.GetType(), GetType());
            return true;
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
            lock (_errors)
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
            lock (_errors)
                return GetErrorsInternal();
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        public Task ValidateAsync(string propertyName)
        {
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
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            var oldHandler = Interlocked.Exchange(ref _weakPropertyHandler, DisposedHandler);
            if (ReferenceEquals(DisposedHandler, oldHandler))
                return;
            OnDispose();
            if (Context != null)
            {
                var notifyPropertyChanged = Instance as INotifyPropertyChanged;
                if (notifyPropertyChanged == null)
                {
                    notifyPropertyChanged = Context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
                    if (notifyPropertyChanged != null)
                        notifyPropertyChanged.PropertyChanged -= oldHandler;
                }
                else
                    notifyPropertyChanged.PropertyChanged -= oldHandler;
            }
            ErrorsChanged = null;
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
        protected Dictionary<string, IList<object>> Errors
        {
            get { return _errors; }
        }

        /// <summary>
        ///     Gets the object to validate.
        /// </summary>
        protected object Instance
        {
            get
            {
                if (Context == null)
                    return null;
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
            get { return _runningTask != null && _runningTask.Count != 0; }
        }

        /// <summary>
        /// Gets the current sync object to access to dictionary.
        /// </summary>
        protected object Locker
        {
            get { return _errors; }
        }

        #endregion

        #region Virtual-abstract methods

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
        [NotNull]
        protected virtual IDictionary<string, IList<object>> GetErrorsInternal()
        {
            return new Dictionary<string, IList<object>>(_errors);
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
            if (_errors.Count == 0)
                return Empty.Array<object>();

            if (string.IsNullOrEmpty(propertyName))
            {
                var objects = new List<object>();
                foreach (var error in _errors.Values)
                    objects.AddRange(error);
                return objects;
            }

            IList<object> list;
            _errors.TryGetValue(propertyName, out list);
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
            return _errors.Count == 0 && !IsValidating;
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
            lock (_errors)
                keys = _errors.Keys.ToArrayEx();
            for (int index = 0; index < keys.Length; index++)
                UpdateErrors(keys[index], null, false);
        }

        /// <summary>
        ///     Publishes a message.
        /// </summary>
        /// <param name="message">The message instance.</param>
        protected virtual void Publish(object message)
        {
            if (Context == null)
                return;
            var eventPublisher = Context.Instance as IEventPublisher;
            var viewModel = Context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
            if (eventPublisher != null)
                eventPublisher.Publish(this, message);
            if (viewModel != null && !ReferenceEquals(eventPublisher, viewModel))
                viewModel.Publish(this, message);
        }

        /// <summary>
        ///     Raises the <see cref="ErrorsChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property that has new errors.</param>
        /// <param name="isAsyncValidate">Indicates that property was async validation.</param>
        protected virtual void RaiseErrorsChanged(string propertyName, bool isAsyncValidate)
        {
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
        ///     Releases resources held by the object.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <returns> The result of validation.</returns>
        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token);

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>The result of validation.</returns>
        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token);

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
        protected internal void UpdateErrors([NotNull] string propertyName, [CanBeNull] IEnumerable errors,
            bool isAsyncValidate)
        {
            ICollection<string> updatedProperties;
            lock (_errors)
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
        protected static bool MemberNameEqual<T>(string memberName, Func<Expression<Func<T, object>>> getMember)
        {
            return ToolkitExtensions.MemberNameEqual(memberName, getMember);
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        protected static string GetMemberName<T>(Func<Expression<Func<T, object>>> expression)
        {
            return expression.GetMemberName();
        }

        /// <summary>
        ///     Makes sure that the object is initialized.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (Context == null)
                throw ExceptionManager.ValidatorNotInitialized(this);
        }

        private Task Validate(string propertyName)
        {
            if (IsDisposed)
                return Empty.Task;
            var validateAll = string.IsNullOrWhiteSpace(propertyName);
            if (validateAll)
                propertyName = string.Empty;
            lock (_validatingMembers)
            {
                if (_validatingMembers.Contains(propertyName))
                    return Empty.Task;
            }

            AsyncValidationMessage message = null;
            bool isAsync;
            Task<IDictionary<string, IEnumerable>> validationTask;
            var cancellationToken = new CancellationTokenSource();
            try
            {
                validationTask = validateAll
                    ? ValidateInternalAsync(cancellationToken.Token)
                    : ValidateInternalAsync(propertyName, cancellationToken.Token);
                if (validationTask == null)
                    validationTask = EmptyResult;
                if (ReferenceEquals(validationTask, DoNothingResult))
                    return Empty.Task;

                isAsync = !validationTask.IsCompleted;
                if (isAsync)
                {
                    lock (_validatingMembers)
                        _validatingMembers.Add(propertyName);
                    message = new AsyncValidationMessage(propertyName);
                    Publish(message);
                    TraceAsync(true, propertyName);
                }
            }
            finally
            {
                if (message != null)
                {
                    lock (_validatingMembers)
                        _validatingMembers.Remove(propertyName);
                }
            }

            if (validationTask.IsCompleted)
            {
                OnValidated(validationTask, validateAll, propertyName, isAsync, message);
                cancellationToken.Dispose();
                return Empty.Task;
            }
            InitializeRunningTaskDict();
            var value = new CancelableClosure(this, validateAll, propertyName, isAsync, message, cancellationToken);
            CancelableClosure oldClosure;
            lock (_runningTask)
            {
                _runningTask.TryGetValue(propertyName, out oldClosure);
                _runningTask[propertyName] = value;
            }
            if (oldClosure != null)
                oldClosure.Cancel();
            return validationTask.TryExecuteSynchronously(value.Callback);
        }

        /// <summary>
        ///     Sets errors for a property with notification.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="validatorErrors">The collection of errors.</param>
        private ICollection<string> UpdateErrorsInternal([NotNull] string propertyName,
            [CanBeNull] IEnumerable validatorErrors)
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
                    _errors[propertyName] = errors;
                else
                    _errors.Remove(propertyName);
                return new[] { propertyName };
            }

            foreach (string property in mappingProperties)
            {
                if (hasErrors)
                    _errors[property] = errors;
                else
                    _errors.Remove(property);
            }
            return mappingProperties;
        }

        private void OnValidated(Task<IDictionary<string, IEnumerable>> task, bool validateAll, string propertyName, bool isAsync, AsyncValidationMessage message)
        {
            Exception exception = null;
            HashSet<string> properties = null;
            try
            {
                if (ReferenceEquals(task.Result, DoNothingResult.Result))
                    return;

                properties = new HashSet<string>(StringComparer.Ordinal);
                lock (_errors)
                {
                    //Clearing old errors
                    if (validateAll)
                    {
                        var keys = _errors.Keys.ToArrayEx();
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
                lock (_validatingMembers)
                {
                    _validatingMembers.Add(propertyName);
                    _validatingMembers.AddRange(properties);
                }
                foreach (var property in properties)
                    RaiseErrorsChanged(property, isAsync);
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                if (isAsync)
                {
                    if (message != null)
                        message.SetCompleted(exception, false);
                    TraceAsync(false, propertyName);
                }
                lock (_validatingMembers)
                {
                    _validatingMembers.Remove(propertyName);
                    if (properties != null)
                        _validatingMembers.RemoveRange(properties);
                }
            }
        }

        private void OnCanceled(string propertyName, AsyncValidationMessage message)
        {
            lock (_validatingMembers)
                _validatingMembers.Add(propertyName);
            message.SetCompleted(null, true);
            TraceAsync(false, propertyName);
            lock (_validatingMembers)
                _validatingMembers.Remove(propertyName);
        }

        private void InitializeWeakHandler()
        {
            if (_weakPropertyHandler == null)
                _weakPropertyHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this,
                    (@base, o, arg3) => @base.OnInstancePropertyChanged(arg3));
        }

        private void InitializeRunningTaskDict()
        {
            if (_runningTask == null)
                Interlocked.CompareExchange(ref _runningTask,
                    new Dictionary<string, CancelableClosure>(StringComparer.Ordinal), null);
        }

        private void OnInstancePropertyChanged(PropertyChangedEventArgs args)
        {
            var context = Context;
            if (ValidateOnPropertyChanged && context != null && !IsDisposed)
                ValidateAsync(args.PropertyName).WithTaskExceptionHandler(this, context.ServiceProvider as IIocContainer);
        }

        private void TraceAsync(bool start, string propertyName)
        {
            if (Tracer.TraceInformation)
                Tracer.Info((start ? "Start" : "Finish") + " asynchronous validation, property name '{0}', validator '{1}'",
                    propertyName, GetType());
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
        protected static bool MemberNameEqual(string memberName, [NotNull] Func<Expression<Func<T, object>>> getMember)
        {
            return ToolkitExtensions.MemberNameEqual(memberName, getMember);
        }

        /// <summary>
        ///     Gets member name from the specified expression.
        /// </summary>
        /// <param name="expression">The specified expression.</param>
        /// <returns>The member name.</returns>
        [Pure]
        protected static string GetMemberName(Func<Expression<Func<T, object>>> expression)
        {
            return expression.GetMemberName();
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