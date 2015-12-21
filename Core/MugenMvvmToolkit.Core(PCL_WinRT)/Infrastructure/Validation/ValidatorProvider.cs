#region Copyright

// ****************************************************************************
// <copyright file="ValidatorProvider.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    public class ValidatorProvider : IValidatorProvider
    {
        #region Fields

        private readonly Dictionary<Type, Type> _validators;

        #endregion

        #region Constructors

        public ValidatorProvider()
            : this(true)
        {
        }

        public ValidatorProvider(bool registerDefaultValidators)
        {
            _validators = new Dictionary<Type, Type>();
            if (registerDefaultValidators)
            {
                Register(typeof(ValidatableViewModelValidator));
                Register(typeof(DataAnnotationValidatior));
            }
        }

        #endregion

        #region Implementation of IValidatorProvider

        public void Register(Type validatorType)
        {
            Should.BeOfType<IValidator>(validatorType, "validatorType");
            lock (_validators)
                _validators[validatorType] = GetSupportedType(validatorType);
        }

        public bool IsRegistered(Type validatorType)
        {
            Should.BeOfType<IValidator>(validatorType, "validatorType");
            lock (_validators)
                return _validators.ContainsKey(validatorType);
        }

        public bool Unregister(Type validatorType)
        {
            Should.BeOfType<IValidator>(validatorType, "validatorType");
            lock (_validators)
                return _validators.Remove(validatorType);
        }

        public IList<Type> GetValidatorTypes()
        {
            lock (_validators)
                return _validators.Keys.ToArrayEx();
        }

        public IList<IValidator> GetValidators(IValidatorContext context)
        {
            Should.NotBeNull(context, nameof(context));
            var validators = new List<IValidator>();
            var instanceType = context.Instance.GetType();
            lock (_validators)
            {
                foreach (var type in _validators)
                {
                    if (!type.Value.IsAssignableFrom(instanceType))
                        continue;
                    var validator = GetValidator(type.Key, context);
                    if (validator != null)
                        validators.Add(validator);
                }
                return validators;
            }
        }

        public IValidatorAggregator GetValidatorAggregator()
        {
            return GetValidatorAggregatorInternal();
        }

        #endregion

        #region Methods

        [CanBeNull]
        protected virtual IValidator GetValidator([NotNull] Type validatorType, [NotNull] IValidatorContext context)
        {
            if (context.Instance is IValidatableViewModel)
            {
                var viewModel = context.ValidationMetadata.GetData(ViewModelConstants.ViewModel);
                if (typeof(ValidatableViewModelValidator).IsAssignableFrom(validatorType))
                {
                    if (ReferenceEquals(context.Instance, viewModel))
                        return null;
                }
                else
                {
                    if (!ReferenceEquals(context.Instance, viewModel) && _validators.ContainsKey(typeof(ValidatableViewModelValidator)))
                        return null;
                }
            }
            IServiceProvider serviceProvider = context.ServiceProvider ?? ServiceProvider.IocContainer;
            IValidator validator = serviceProvider == null
                ? (IValidator)Activator.CreateInstance(validatorType)
                : (IValidator)serviceProvider.GetService(validatorType);
            if (validator.Initialize(context))
                return validator;
            validator.Dispose();
            return null;
        }

        [NotNull]
        protected virtual IValidatorAggregator GetValidatorAggregatorInternal()
        {
            ValidatableViewModel viewModel;
            IIocContainer iocContainer = ServiceProvider.IocContainer;
            IViewModelProvider viewModelProvider;
            if (iocContainer != null && iocContainer.TryGet(out viewModelProvider))
                viewModel = viewModelProvider.GetViewModel<ValidatableViewModel>();
            else
                viewModel = new ValidatableViewModel();
            viewModel.ValidatorProvider = this;
            return viewModel;
        }

        private static Type GetSupportedType(Type validatorType)
        {
            while (validatorType != null)
            {
#if PCL_WINRT
                var typeInfo = validatorType.GetTypeInfo();
                if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(ValidatorBase<>))
                    return typeInfo.GenericTypeArguments[0];
                validatorType = typeInfo.BaseType;
#else
                if (validatorType.IsGenericType && validatorType.GetGenericTypeDefinition() == typeof(ValidatorBase<>))
                    return validatorType.GetGenericArguments()[0];
                validatorType = validatorType.BaseType;
#endif
            }
            return typeof(object);
        }

        #endregion
    }
}
