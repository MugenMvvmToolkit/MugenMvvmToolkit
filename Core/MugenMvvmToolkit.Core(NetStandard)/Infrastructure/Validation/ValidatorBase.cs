#region Copyright

// ****************************************************************************
// <copyright file="ValidatorBase.cs">
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

        protected static readonly Task<IDictionary<string, IEnumerable>> EmptyResult;

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

        protected ValidatorBase()
        {
            _errors = new Dictionary<string, IList<object>>(StringComparer.Ordinal);
            _validatingMembers = new List<string>();
            ValidateOnPropertyChanged = ValidateOnPropertyChangedDefault;
        }

        #endregion

        #region Implementation of IValidator

        public bool IsDisposed => ReferenceEquals(_weakPropertyHandler, DisposedHandler);

        public bool IsInitialized => Context != null;

        public bool IsValid
        {
            get
            {
                if (Context == null)
                    return false;
                return IsValidInternal();
            }
        }

        public bool ValidateOnPropertyChanged { get; set; }

        public IValidatorContext Context => _context;

        public bool Initialize(IValidatorContext context)
        {
            if (IsDisposed)
                throw ExceptionManager.ObjectDisposed(GetType());
            Should.NotBeNull(context, nameof(context));
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

        public IList<object> GetErrors(string propertyName)
        {
            EnsureInitialized();
            lock (_errors)
                return GetErrorsInternal(propertyName);
        }

        public IDictionary<string, IList<object>> GetErrors()
        {
            EnsureInitialized();
            lock (_errors)
                return GetErrorsInternal();
        }

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

        public Task ValidateAsync()
        {
            EnsureInitialized();
            if (IgnoreProperties.Contains(string.Empty))
                return Empty.Task;
            return Validate(string.Empty);
        }

        public void CancelValidation()
        {
            lock (_runningTask)
            {
                var tasks = _runningTask.ToArrayEx();
                _runningTask.Clear();
                for (int i = 0; i < tasks.Length; i++)
                    tasks[i].Value.Cancel();
            }
        }

        public void ClearErrors(string propertyName)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(propertyName))
                ClearErrorsInternal();
            else
                ClearErrorsInternal(propertyName);
        }

        public void ClearErrors()
        {
            EnsureInitialized();
            ClearErrorsInternal();
        }

        public bool HasErrors => !IsValid;

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }

        public virtual event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

#if NONOTIFYDATAERROR
        string IDataErrorInfo.this[string columnName] => GetErrors(columnName).FirstOrDefault()?.ToString();

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

        public static bool ValidateOnPropertyChangedDefault { get; set; }

        protected Dictionary<string, IList<object>> Errors => _errors;

        protected object Instance => Context?.Instance;

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

        protected bool IsValidating => _runningTask != null && _runningTask.Count != 0;

        protected object Locker => _errors;

        #endregion

        #region Virtual-abstract methods

        protected virtual void OnInitialized(IValidatorContext context)
        {
        }

        [NotNull]
        protected virtual IDictionary<string, IList<object>> GetErrorsInternal()
        {
            return new Dictionary<string, IList<object>>(_errors);
        }

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

        protected virtual bool IsValidInternal()
        {
            return _errors.Count == 0 && !IsValidating;
        }

        protected virtual void ClearErrorsInternal(string propertyName)
        {
            UpdateErrors(propertyName, null, false);
        }

        protected virtual void ClearErrorsInternal()
        {
            string[] keys;
            lock (_errors)
                keys = _errors.Keys.ToArrayEx();
            for (int index = 0; index < keys.Length; index++)
                UpdateErrors(keys[index], null, false);
        }

        protected virtual void Publish(object message)
        {
            if (Context == null)
                return;
            var eventPublisher = Context.Instance as IEventPublisher;
            var viewModel = Context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
            eventPublisher?.Publish(this, message);
            if (viewModel != null && !ReferenceEquals(eventPublisher, viewModel))
                viewModel.Publish(this, message);
        }

        protected virtual void RaiseErrorsChanged(string propertyName, bool isAsyncValidate)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        internal virtual bool CanValidateContext(IValidatorContext validatorContext)
        {
            return true;
        }

        protected virtual bool CanValidateInternal(IValidatorContext validatorContext)
        {
            return true;
        }

        protected virtual void OnDispose()
        {
        }

        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName, CancellationToken token);

        [CanBeNull]
        protected abstract Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(CancellationToken token);

        #endregion

        #region Methods

        protected static Task<IDictionary<string, IEnumerable>> FromResult(IDictionary<string, IEnumerable> result)
        {
            return ToolkitExtensions.FromResult(result);
        }

        protected internal void UpdateErrors([NotNull] string propertyName, [CanBeNull] IEnumerable errors,
            bool isAsyncValidate)
        {
            ICollection<string> updatedProperties;
            lock (_errors)
                updatedProperties = UpdateErrorsInternal(propertyName, errors);
            foreach (string name in updatedProperties)
                RaiseErrorsChanged(name, isAsyncValidate);
        }

        [Pure]
        protected static bool MemberNameEqual<T>(string memberName, Func<Expression<Func<T, object>>> getMember)
        {
            return ToolkitExtensions.MemberNameEqual(memberName, getMember);
        }

        [Pure]
        protected static string GetMemberName<T>(Func<Expression<Func<T, object>>> expression)
        {
            return expression.GetMemberName();
        }

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
            oldClosure?.Cancel();
            return validationTask.TryExecuteSynchronously(value.Callback);
        }

        private ICollection<string> UpdateErrorsInternal([NotNull] string propertyName,
            [CanBeNull] IEnumerable validatorErrors)
        {
            Should.NotBeNull(propertyName, nameof(propertyName));
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
                    message?.SetCompleted(exception, false);
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

    public abstract class ValidatorBase<T> : ValidatorBase
    {
        #region Properties

        protected new T Instance => (T)base.Instance;

        #endregion

        #region Methods

        [Pure]
        protected static bool MemberNameEqual(string memberName, [NotNull] Func<Expression<Func<T, object>>> getMember)
        {
            return ToolkitExtensions.MemberNameEqual(memberName, getMember);
        }

        [Pure]
        protected static string GetMemberName(Func<Expression<Func<T, object>>> expression)
        {
            return expression.GetMemberName();
        }

        #endregion

        #region Overrides of ValidatorBase

        internal override bool CanValidateContext(IValidatorContext validatorContext)
        {
            return validatorContext.Instance is T;
        }

        #endregion
    }
}
