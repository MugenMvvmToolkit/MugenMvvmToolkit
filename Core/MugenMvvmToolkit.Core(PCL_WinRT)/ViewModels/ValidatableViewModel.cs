#region Copyright

// ****************************************************************************
// <copyright file="ValidatableViewModel.cs">
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Models.Validation;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 6)]
    public class ValidatableViewModel : CloseableViewModel, IValidatableViewModel
    {
        #region Fields

        private static readonly Action<ValidatableViewModel, DataErrorsChangedEventArgs> RaiseErrorsChangedDelegate;

        private readonly object _locker;
        private readonly Dictionary<object, List<IValidator>> _instanceToValidators;
        private readonly Dictionary<string, ICollection<string>> _propertyMappings;
        private readonly HashSet<string> _ignoreProperties;
        private readonly ManualValidator _validator;

        private EventHandler<DataErrorsChangedEventArgs> _weakHandler;
        private IValidatorProvider _validatorProvider;
        private Func<object, IValidatorContext> _createContext;

        #endregion

        #region Constructors

        static ValidatableViewModel()
        {
            RaiseErrorsChangedDelegate = RaiseErrorsChangedStatic;
        }

        public ValidatableViewModel()
        {
            Type type = GetType();
            _locker = new object();
            _instanceToValidators = new Dictionary<object, List<IValidator>>();
            _weakHandler = ReflectionExtensions.MakeWeakErrorsChangedHandler(this, (validator, o, arg3) => validator.RaiseErrorsChanged(arg3));
            _validator = new ManualValidator();

            _createContext = CreateContextInternal;
            _propertyMappings = type.GetViewModelToModelProperties();
            _ignoreProperties = type.GetIgnoreProperties();
            _validator.Initialize(new ValidatorContext(this, _propertyMappings, _ignoreProperties, Settings.Metadata));
            AddValidator(_validator);
        }

        #endregion

        #region Properties

        protected Dictionary<object, List<IValidator>> InstanceToValidators => _instanceToValidators;

        protected internal IValidatorProvider ValidatorProvider
        {
            get { return _validatorProvider; }
            set
            {
                Should.PropertyNotBeNull(value);
                _validatorProvider = value;
            }
        }

        #endregion

        #region Methods

        public TValidator AddValidator<TValidator>([NotNull] object instanceToValidate)
            where TValidator : IValidator
        {
            return ToolkitExtensions.AddValidator<TValidator>(this, instanceToValidate);
        }

        protected void SetValidatorErrors<TModel>(Func<Expression<Func<TModel, object>>> expresssion, params object[] errors)
        {
            ToolkitExtensions.SetValidatorErrors(this, expresssion, errors);
        }

        protected void SetValidatorErrors(string property, params object[] errors)
        {
            ToolkitExtensions.SetValidatorErrors(this, property, errors);
        }

        protected Task ValidateAsync<TModel>(Func<Expression<Func<TModel, object>>> getProperty)
        {
            Should.NotBeNull(getProperty, nameof(getProperty));
            return ValidateAsync(getProperty.GetMemberName());
        }

        protected void AddIgnoreProperty<TModel>(Func<Expression<Func<TModel, object>>> getProperty)
        {
            IgnoreProperties.Add(getProperty.GetMemberName());
        }

        protected void RemoveIgnoreProperty<TModel>(Func<Expression<Func<TModel, object>>> getProperty)
        {
            IgnoreProperties.Remove(getProperty.GetMemberName());
        }

        protected virtual void ClearErrorsInternal(string propertyName)
        {
            foreach (var validators in _instanceToValidators.Values)
                for (int index = 0; index < validators.Count; index++)
                    validators[index].ClearErrors(propertyName);
        }

        protected virtual void ClearErrorsInternal()
        {
            foreach (var validators in _instanceToValidators.Values)
                for (int index = 0; index < validators.Count; index++)
                    validators[index].ClearErrors();
        }

        protected virtual IList<object> GetErrorsInternal(string propertyName)
        {
            var listResults = new List<object>();
            foreach (var validators in _instanceToValidators.Values)
            {
                for (int index = 0; index < validators.Count; index++)
                    listResults.AddRange(validators[index].GetErrors(propertyName));
            }
            return listResults;
        }

        protected virtual IDictionary<string, IList<object>> GetErrorsInternal()
        {
            var errors = new List<IDictionary<string, IList<object>>>();
            foreach (var validators in _instanceToValidators.Values)
            {
                for (int index = 0; index < validators.Count; index++)
                    errors.Add(validators[index].GetErrors());
            }
            return ToolkitExtensions.MergeDictionaries(errors);
        }

        protected virtual void AddValidatorInternal(IValidator validator)
        {
            if (validator.Context == null)
                throw ExceptionManager.ValidatorNotInitialized("validator");
            //To prevent recursive validation call.
            if (validator is ValidatableViewModelValidator && ReferenceEquals(validator.Context.Instance, this))
            {
                validator.Dispose();
                return;
            }
            List<IValidator> validators;
            _instanceToValidators.TryGetValue(validator.Context.Instance, out validators);
            if (validators == null)
            {
                validators = new List<IValidator>();
                _instanceToValidators[validator.Context.Instance] = validators;
            }
            validators.Add(validator);
            validator.ErrorsChanged += _weakHandler;
            validator.ValidateAsync();
        }

        protected virtual bool RemoveValidatorInternal(IValidator validator)
        {
            if (validator.Context == null)
                throw ExceptionManager.ValidatorNotInitialized("validator");
            List<IValidator> validators;
            if (!_instanceToValidators.TryGetValue(validator.Context.Instance, out validators) ||
                validators == null || !validators.Contains(validator))
                return false;
            validator.ErrorsChanged -= _weakHandler;
            validator.Dispose();
            return validators.Remove(validator);
        }

        protected virtual void AddInstanceInternal(object instanceToValidate)
        {
            if (_instanceToValidators.ContainsKey(instanceToValidate))
                RemoveInstanceInternal(instanceToValidate);
            IValidatorContext context = CreateContext(instanceToValidate);
            IList<IValidator> validators = ValidatorProvider.GetValidators(context);
            for (int index = 0; index < validators.Count; index++)
                AddValidatorInternal(validators[index]);
        }

        protected virtual bool RemoveInstanceInternal(object instanceToValidate)
        {
            List<IValidator> validators;
            if (!_instanceToValidators.TryGetValue(instanceToValidate, out validators))
                return false;
            if (validators != null)
            {
                for (int index = 0; index < validators.Count; index++)
                {
                    var validator = validators[index];
                    if (validator == _validator)
                        continue;
                    if (RemoveValidatorInternal(validator))
                        index--;
                }
            }
            return this == instanceToValidate || _instanceToValidators.Remove(instanceToValidate);
        }

        protected virtual bool IsValidInternal()
        {
            foreach (var validators in _instanceToValidators.Values)
            {
                for (int index = 0; index < validators.Count; index++)
                {
                    if (!validators[index].IsValid)
                        return false;
                }
            }
            return true;
        }

        protected virtual Task ValidateInstanceInternal(object instanceToValidate)
        {
            List<IValidator> list;
            if (!_instanceToValidators.TryGetValue(instanceToValidate, out list) || list.Count == 0)
                return Empty.Task;
            if (list.Count == 1)
                return list[0].ValidateAsync();
            return ToolkitExtensions.WhenAll(list.ToArrayEx(validator => validator.ValidateAsync()));
        }

        protected virtual Task ValidateInternal(string propertyName)
        {
            var tasks = new List<Task>();
            foreach (var validators in _instanceToValidators.Values)
            {
                for (int index = 0; index < validators.Count; index++)
                    tasks.Add(validators[index].ValidateAsync(propertyName));
            }
            if (tasks.Count == 1)
                return tasks[0];
            return ToolkitExtensions.WhenAll(tasks.ToArrayEx());
        }

        protected virtual Task ValidateInternal()
        {
            var tasks = new List<Task>();
            foreach (var validators in _instanceToValidators.Values)
            {
                for (int index = 0; index < validators.Count; index++)
                    tasks.Add(validators[index].ValidateAsync());
            }
            if (tasks.Count == 1)
                return tasks[0];
            return ToolkitExtensions.WhenAll(tasks.ToArrayEx());
        }

        protected virtual void OnHandleAsyncValidationMessage(object sender, AsyncValidationMessage message)
        {
        }

        protected virtual void RaiseErrorsChanged(DataErrorsChangedEventArgs args)
        {
            OnPropertyChanged(Empty.HasErrorsChangedArgs);
            OnPropertyChanged(Empty.IsValidChangedArgs);
            OnPropertyChanged(Empty.IndexerPropertyChangedArgs);
            if (ErrorsChanged != null)
                ThreadManager.Invoke(Settings.EventExecutionMode, this, args, RaiseErrorsChangedDelegate);
#if NONOTIFYDATAERROR
            string ignoreProperty = args.PropertyName ?? string.Empty;
            lock (_locker)
            {
                //Disable validation to prevent cycle
                var contains = IgnoreProperties.Contains(ignoreProperty);
                if (!contains)
                    IgnoreProperties.Add(ignoreProperty);
                try
                {
                    OnPropertyChanged(ignoreProperty);
                }
                finally
                {
                    if (!contains)
                        IgnoreProperties.Remove(ignoreProperty);
                }
            }
#endif
        }

        private static void RaiseErrorsChangedStatic(ValidatableViewModel @this, DataErrorsChangedEventArgs args)
        {
            @this.ErrorsChanged?.Invoke(@this, args);
        }

        private IValidatorContext CreateContextInternal(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            EnsureNotDisposed();
            var ctx = new ValidatorContext(instanceToValidate, PropertyMappings, IgnoreProperties, Settings.Metadata,
                this.GetIocContainer(true, false));
            ctx.ValidationMetadata.AddOrUpdate(ViewModelConstants.ViewModel, this);
            return ctx;
        }

        private void HandleValidationInternal(object sender, object message)
        {
            var validationMessage = message as AsyncValidationMessage;
            if (validationMessage != null)
                OnHandleAsyncValidationMessage(sender, validationMessage);
        }

        #endregion

        #region Implementation of IValidatableViewModel

        public virtual Func<object, IValidatorContext> CreateContext
        {
            get { return _createContext; }
            set
            {
                if (Equals(_createContext, value))
                    return;
                _createContext = value;
                OnPropertyChanged();
            }
        }

        public IDictionary<string, ICollection<string>> PropertyMappings => _propertyMappings;

        public ICollection<string> IgnoreProperties => _ignoreProperties;

        public ManualValidator Validator => _validator;

        public bool IsValid
        {
            get
            {
                if (IsDisposed)
                    return false;
                lock (_locker)
                    return IsValidInternal();
            }
        }

        public IList<IValidator> GetValidators()
        {
            lock (_locker)
            {
                var validators = new List<IValidator>();
                foreach (var value in _instanceToValidators.Values)
                    validators.AddRange(value);
                return validators;
            }
        }

        public void AddValidator(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            EnsureNotDisposed();
            lock (_locker)
                AddValidatorInternal(validator);
        }

        public bool RemoveValidator(IValidator validator)
        {
            Should.NotBeNull(validator, nameof(validator));
            EnsureNotDisposed();
            bool result;
            lock (_locker)
                result = RemoveValidatorInternal(validator);
            if (result)
                RaiseErrorsChanged(Empty.EmptyDataErrorsChangedArgs);
            return result;
        }

        public void AddInstance(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            EnsureNotDisposed();
            lock (_locker)
                AddInstanceInternal(instanceToValidate);
        }

        public bool RemoveInstance(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            EnsureNotDisposed();
            bool result;
            lock (_locker)
                result = RemoveInstanceInternal(instanceToValidate);
            if (result)
                RaiseErrorsChanged(Empty.EmptyDataErrorsChangedArgs);
            return result;
        }

        public Task ValidateInstanceAsync(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, nameof(instanceToValidate));
            EnsureNotDisposed();
            Task task;
            lock (_locker)
                task = ValidateInstanceInternal(instanceToValidate);
            return task.WithTaskExceptionHandler(this);
        }

        public Task ValidateAsync(string propertyName)
        {
            Should.NotBeNull(propertyName, nameof(propertyName));
            Task task;
            lock (_locker)
                task = ValidateInternal(propertyName);
            return task.WithTaskExceptionHandler(this);
        }

        public Task ValidateAsync()
        {
            Task task;
            lock (_locker)
                task = ValidateInternal();
            return task.WithTaskExceptionHandler(this);
        }

        public IList<object> GetErrors(string propertyName)
        {
            lock (_locker)
                return GetErrorsInternal(propertyName);
        }

        public IDictionary<string, IList<object>> GetErrors()
        {
            lock (_locker)
                return GetErrorsInternal();
        }

        public void ClearErrors(string propertyName)
        {
            Should.NotBeNull(propertyName, nameof(propertyName));
            lock (_locker)
                ClearErrorsInternal(propertyName);
        }

        public void ClearErrors()
        {
            lock (_locker)
                ClearErrorsInternal();
        }

        public bool HasErrors => !IsValid;

        IEnumerable INotifyDataErrorInfo.GetErrors(string propertyName)
        {
            return GetErrors(propertyName);
        }

        public virtual event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

#if NONOTIFYDATAERROR
        public string this[string columnName]
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
                if (errors == null || errors.Count == 0)
                    return null;
                return string.Join(Environment.NewLine, errors);
            }
        }
#else
        public IList<object> this[string propertyName]
        {
            get
            {
                if (ApplicationSettings.GetAllErrorsIndexerProperty == propertyName)
                    propertyName = string.Empty;
                return GetErrors(propertyName);
            }
        }
#endif
        #endregion

        #region Overrides of ViewModelBase

        internal override void HandleInternal(object sender, object message)
        {
            HandleValidationInternal(sender, message);
            base.HandleInternal(sender, message);
        }

        internal override void OnInitializedInternal()
        {
            if (ValidatorProvider == null)
                ValidatorProvider = IocContainer.Get<IValidatorProvider>();
            AddInstance(this);
            base.OnInitializedInternal();
        }

        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                object[] toRemove;
                lock (_locker)
                    toRemove = _instanceToValidators.Keys.ToArrayEx();

                for (int index = 0; index < toRemove.Length; index++)
                    RemoveInstance(toRemove[index]);
                Validator.Dispose();
                ErrorsChanged = null;
                _weakHandler = null;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}
