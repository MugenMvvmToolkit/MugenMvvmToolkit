#region Copyright
// ****************************************************************************
// <copyright file="ValidatorProvider.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represent the factory for create <see cref="IValidator" />.
    /// </summary>
    public class ValidatorProvider : DisposableObject, IValidatorProvider
    {
        #region Fields

        private readonly IDictionary<Type, List<IValidator>> _validatorPrototypes;
        private IServiceProvider _serviceProvider;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorProvider" /> class.
        /// </summary>
        private ValidatorProvider(bool registerDefaultValidators, [CanBeNull] IServiceProvider serviceProvider, IDictionary<Type, List<IValidator>> validatorPrototypes)
        {
            _validatorPrototypes = validatorPrototypes;
            _serviceProvider = serviceProvider;
            if (!registerDefaultValidators)
                return;
            Register<ValidatableViewModelValidator>();
            Register<ValidationElementValidator>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidatorProvider" /> class.
        /// </summary>
        public ValidatorProvider(bool registerDefaultValidators, [CanBeNull] IServiceProvider serviceProvider)
            : this(registerDefaultValidators, serviceProvider ?? MugenMvvmToolkit.ServiceProvider.IocContainer, new Dictionary<Type, List<IValidator>>())
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the service provider.
        /// </summary>
        public virtual IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    return MugenMvvmToolkit.ServiceProvider.IocContainer;
                return _serviceProvider;
            }
            set { _serviceProvider = value; }
        }

        #endregion

        #region Implementation of IValidatorProvider

        /// <summary>
        ///     Registers the specified validator using the type.
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        public void Register<TValidator>() where TValidator : IValidator
        {
            IValidator validator = ServiceProvider == null
                ? (IValidator)Activator.CreateInstance(typeof(TValidator))
                : ServiceProvider.GetService<TValidator>();
            Register(validator);
        }

        /// <summary>
        ///     Registers the specified validator.
        /// </summary>
        /// <param name="validator">The specified validator</param>
        public void Register(IValidator validator)
        {
            Should.NotBeNull(validator, "validator");
            lock (_validatorPrototypes)
            {
                Type type = validator.GetType();
                List<IValidator> validators;
                _validatorPrototypes.TryGetValue(type, out validators);
                if (validators != null && validators.Count != 0 && validator.IsUnique)
                    throw ExceptionManager.DuplicateValidator(type);
                if (validators == null)
                    validators = new List<IValidator>();
                validators.Add(validator);
                _validatorPrototypes[type] = validators;
            }
        }

        /// <summary>
        ///     Unregisters the specified validator use type.
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        public bool Unregister<TValidator>() where TValidator : IValidator
        {
            lock (_validatorPrototypes)
                return _validatorPrototypes.Remove(typeof(TValidator));
        }

        /// <summary>
        ///     Determines whether the specified validator is registered
        /// </summary>
        /// <typeparam name="TValidator">The type of validator.</typeparam>
        public bool IsRegistered<TValidator>() where TValidator : IValidator
        {
            return IsRegistered(typeof(TValidator));
        }

        /// <summary>
        ///     Determines whether the specified validator is registered
        /// </summary>
        public bool IsRegistered(Type type)
        {
            lock (_validatorPrototypes)
                return _validatorPrototypes.ContainsKey(type);
        }

        /// <summary>
        ///     Gets the series of validators for the specified instance.
        /// </summary>
        /// <param name="context">The specified IValidatorContext.</param>
        /// <returns>A series instances of validators.</returns>
        public IList<IValidator> GetValidators(IValidatorContext context)
        {
            Should.NotBeNull(context, "context");
            lock (_validatorPrototypes)
            {
                var listResults = new List<IValidator>();
                foreach (var values in _validatorPrototypes.Values)
                {
                    for (int index = 0; index < values.Count; index++)
                    {
                        var value = values[index];
                        if (!value.CanValidate(context))
                            continue;
                        IValidator validator = value.Clone();
                        validator.Initialize(context);
                        listResults.Add(validator);
                    }
                }
                return listResults;
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="IValidatorAggregator" />.
        /// </summary>
        public IValidatorAggregator GetValidatorAggregator()
        {
            return GetValidatorAggregatorInternal();
        }

        /// <summary>
        ///     Creates a new validator-factory that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new validator-factory that is a copy of this instance.
        /// </returns>
        public IValidatorProvider Clone()
        {
            lock (_validatorPrototypes)
            {
                var validators = new Dictionary<Type, List<IValidator>>();
                foreach (var prototype in _validatorPrototypes)
                    validators.Add(prototype.Key, new List<IValidator>(prototype.Value));
                return new ValidatorProvider(false, ServiceProvider, validators);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of <see cref="IValidatorAggregator" />.
        /// </summary>
        [NotNull]
        protected virtual IValidatorAggregator GetValidatorAggregatorInternal()
        {
            ValidatableViewModel viewModel;
            var iocContainer = ServiceProvider as IIocContainer;
            IViewModelProvider viewModelProvider;
            if (iocContainer != null && iocContainer.TryGet(out viewModelProvider))
                viewModel = viewModelProvider.GetViewModel<ValidatableViewModel>();
            else
                viewModel = new ValidatableViewModel();
            viewModel.ValidatorProvider = this;
            return viewModel;
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
                lock (_validatorPrototypes)
                {
                    foreach (var prototype in _validatorPrototypes)
                    {
                        for (int index = 0; index < prototype.Value.Count; index++)
                            prototype.Value[index].Dispose();
                    }
                    _validatorPrototypes.Clear();
                }
            }
            base.OnDispose(disposing);
        }

        #endregion
    }
}