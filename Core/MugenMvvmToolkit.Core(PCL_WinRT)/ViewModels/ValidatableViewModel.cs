#region Copyright

// ****************************************************************************
// <copyright file="ValidatableViewModel.cs">
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Models.Validation;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the view-model which has validation support.
    /// </summary>
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

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatableViewModel" /> class.
        /// </summary>
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

        /// <summary>
        ///     Gets the dictionary that contains mapping from an instance to validators.
        /// </summary>
        protected Dictionary<object, List<IValidator>> InstanceToValidators
        {
            get { return _instanceToValidators; }
        }

        /// <summary>
        /// Gets or sets the current <see cref="IValidatorProvider"/>.
        /// </summary>
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

        /// <summary>
        ///     Adds the specified validator.
        /// </summary>
        public TValidator AddValidator<TValidator>([NotNull] object instanceToValidate)
            where TValidator : IValidator
        {
            return ToolkitExtensions.AddValidator<TValidator>(this, instanceToValidate);
        }

        /// <summary>
        ///     Sets errors for a property using the <see cref="Validator"/>.
        /// </summary>
        /// <param name="propertyExpresssion">The expression for the property</param>
        /// <param name="errors">The collection of errors</param>
        protected void SetValidatorErrors<T>(Expression<Func<T>> propertyExpresssion, params object[] errors)
        {
            ToolkitExtensions.SetValidatorErrors(this, propertyExpresssion, errors);
        }

        /// <summary>
        ///     Sets errors for a property using the <see cref="Validator"/>.
        /// </summary>
        /// <param name="property">The property name</param>
        /// <param name="errors">The collection of errors</param>
        protected void SetValidatorErrors(string property, params object[] errors)
        {
            ToolkitExtensions.SetValidatorErrors(this, property, errors);
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        [SuppressTaskBusyHandler]
        protected Task ValidateAsync<T>(Expression<Func<T>> getProperty)
        {
            Should.NotBeNull(getProperty, "getProperty");
            return ValidateAsync(ToolkitExtensions.GetMemberName(getProperty));
        }

        /// <summary>
        ///     Adds a property name to the <see cref="IgnoreProperties" />.
        /// </summary>
        protected void AddIgnoreProperty<T>(Expression<Func<T>> getProperty)
        {
            IgnoreProperties.Add(getProperty.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Removes a property name to the <see cref="IgnoreProperties" />.
        /// </summary>
        protected void RemoveIgnoreProperty<T>(Expression<Func<T>> getProperty)
        {
            IgnoreProperties.Remove(getProperty.GetMemberInfo().Name);
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        protected virtual void ClearErrorsInternal(string propertyName)
        {
            foreach (var validators in _instanceToValidators.Values)
                for (int index = 0; index < validators.Count; index++)
                    validators[index].ClearErrors(propertyName);
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        protected virtual void ClearErrorsInternal()
        {
            foreach (var validators in _instanceToValidators.Values)
                for (int index = 0; index < validators.Count; index++)
                    validators[index].ClearErrors();
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

        /// <summary>
        ///     Gets all validation errors.
        /// </summary>
        /// <returns>
        ///     The validation errors.
        /// </returns>
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

        /// <summary>
        ///     Adds the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
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

        /// <summary>
        ///     Removes the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
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

        /// <summary>
        ///     Adds the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        protected virtual void AddInstanceInternal(object instanceToValidate)
        {
            if (_instanceToValidators.ContainsKey(instanceToValidate))
                RemoveInstanceInternal(instanceToValidate);
            IValidatorContext context = CreateContext(instanceToValidate);
            IList<IValidator> validators = ValidatorProvider.GetValidators(context);
            for (int index = 0; index < validators.Count; index++)
                AddValidatorInternal(validators[index]);
        }

        /// <summary>
        ///     Adds the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
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

        /// <summary>
        ///     Determines whether the current model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current model is valid, otherwise <c>false</c>.
        /// </returns>
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

        /// <summary>
        ///     Updates information about errors in the specified instance.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        [SuppressTaskBusyHandler]
        protected virtual Task ValidateInstanceInternal(object instanceToValidate)
        {
            List<IValidator> list;
            if (!_instanceToValidators.TryGetValue(instanceToValidate, out list) || list.Count == 0)
                return Empty.Task;
            if (list.Count == 1)
                return list[0].ValidateAsync();
            return ToolkitExtensions.WhenAll(list.ToArrayEx(validator => validator.ValidateAsync()));
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        [SuppressTaskBusyHandler]
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

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        [SuppressTaskBusyHandler]
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

        /// <summary>
        ///     Occurs when processing an asynchronous validation message.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="message">Information about event.</param>
        protected virtual void OnHandleAsyncValidationMessage(object sender, AsyncValidationMessage message)
        {
            if (message.IsEndOperation)
            {
                ((IHandler<object>)this).Handle(this, new EndBusyMessage(message.Id));
                return;
            }
            ((IHandler<object>)this).Handle(this, new BeginBusyMessage(message.Id, Settings.ValidationBusyMessage));
        }

        /// <summary>
        ///     Raises the <see cref="ErrorsChanged"/> event.
        /// </summary>
        /// <param name="args">The event args.</param>
        protected virtual void RaiseErrorsChanged(DataErrorsChangedEventArgs args)
        {
            OnPropertyChanged(Empty.HasErrorsChangedArgs);
            OnPropertyChanged(Empty.IsValidChangedArgs);
            if (ErrorsChanged != null)
                ThreadManager.Invoke(Settings.EventExecutionMode, this, args, RaiseErrorsChangedDelegate);
        }

        private static void RaiseErrorsChangedStatic(ValidatableViewModel @this, DataErrorsChangedEventArgs args)
        {
            var handler = @this.ErrorsChanged;
            if (handler != null)
                handler(@this, args);
#if NONOTIFYDATAERROR
            string ignoreProperty = args.PropertyName ?? string.Empty;
            lock (@this._locker)
            {
                //Disable validation to prevent cycle
                var contains = @this.IgnoreProperties.Contains(ignoreProperty);
                if (!contains)
                    @this.IgnoreProperties.Add(ignoreProperty);
                try
                {
                    @this.OnPropertyChanged(ignoreProperty, ExecutionMode.None);
                }
                finally
                {
                    if (!contains)
                        @this.IgnoreProperties.Remove(ignoreProperty);
                }
            }
#endif
        }

        private IValidatorContext CreateContextInternal(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
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

        /// <summary>
        ///     Gets or sets the delegate that allows to create an instance of <see cref="IValidatorContext" />.
        /// </summary>
        public virtual Func<object, IValidatorContext> CreateContext
        {
            get { return _createContext; }
            set
            {
                if (Equals(_createContext, value))
                    return;
                _createContext = value;
                OnPropertyChanged("CreateContext");
            }
        }

        /// <summary>
        ///     Gets the mapping of model properties.
        ///     <example>
        ///         <code>
        ///       <![CDATA[
        ///        PropertyMappings.Add("ModelProperty", new[]{"ViewModelProperty"});
        ///       ]]>
        ///     </code>
        ///     </example>
        /// </summary>
        public IDictionary<string, ICollection<string>> PropertyMappings
        {
            get { return _propertyMappings; }
        }

        /// <summary>
        ///     Gets the list of properties that will not be validated.
        /// </summary>
        public ICollection<string> IgnoreProperties
        {
            get { return _ignoreProperties; }
        }

        /// <summary>
        ///     Gets the validator that allows to set errors manually.
        /// </summary>
        public ManualValidator Validator
        {
            get { return _validator; }
        }

        /// <summary>
        ///     Determines whether the current view model is valid.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> current view model is valid, otherwise <c>false</c>.
        /// </returns>
        public bool IsValid
        {
            get
            {
                lock (_locker)
                    return IsValidInternal();
            }
        }

        /// <summary>
        ///     Gets the collection of validators.
        /// </summary>
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

        /// <summary>
        ///     Adds the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
        public void AddValidator(IValidator validator)
        {
            Should.NotBeNull(validator, "validator");
            EnsureNotDisposed();
            lock (_locker)
                AddValidatorInternal(validator);
        }

        /// <summary>
        ///     Removes the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator.</param>
        public bool RemoveValidator(IValidator validator)
        {
            Should.NotBeNull(validator, "validator");
            EnsureNotDisposed();
            bool result;
            lock (_locker)
                result = RemoveValidatorInternal(validator);
            if (result)
                RaiseErrorsChanged(Empty.EmptyDataErrorsChangedArgs);
            return result;
        }

        /// <summary>
        ///     Adds the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        public void AddInstance(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            EnsureNotDisposed();
            lock (_locker)
                AddInstanceInternal(instanceToValidate);
        }

        /// <summary>
        ///     Removes the specified instance to validate.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        public bool RemoveInstance(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            EnsureNotDisposed();
            bool result;
            lock (_locker)
                result = RemoveInstanceInternal(instanceToValidate);
            if (result)
                RaiseErrorsChanged(Empty.EmptyDataErrorsChangedArgs);
            return result;
        }

        /// <summary>
        ///     Updates information about errors in the specified instance.
        /// </summary>
        /// <param name="instanceToValidate">The specified instance to validate.</param>
        [SuppressTaskBusyHandler]
        public Task ValidateInstanceAsync(object instanceToValidate)
        {
            Should.NotBeNull(instanceToValidate, "instanceToValidate");
            EnsureNotDisposed();
            Task task;
            lock (_locker)
                task = ValidateInstanceInternal(instanceToValidate);
            return task.WithTaskExceptionHandler(this);
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        [SuppressTaskBusyHandler]
        public Task ValidateAsync(string propertyName)
        {
            Should.NotBeNull(propertyName, "propertyName");
            Task task;
            lock (_locker)
                task = ValidateInternal(propertyName);
            return task.WithTaskExceptionHandler(this);
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        [SuppressTaskBusyHandler]
        public Task ValidateAsync()
        {
            EnsureNotDisposed();
            Task task;
            lock (_locker)
                task = ValidateInternal();
            return task.WithTaskExceptionHandler(this);
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
            lock (_locker)
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
            lock (_locker)
                return GetErrorsInternal();
        }

        /// <summary>
        ///     Clears errors for a property.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        public void ClearErrors(string propertyName)
        {
            Should.NotBeNull(propertyName, "propertyName");
            lock (_locker)
                ClearErrorsInternal(propertyName);
        }

        /// <summary>
        ///     Clears all errors.
        /// </summary>
        public void ClearErrors()
        {
            lock (_locker)
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
                if (errors == null || errors.Count == 0)
                    return null;
                return string.Join(Environment.NewLine, errors);
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