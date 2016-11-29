using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;
using Should;

namespace MugenMvvmToolkit.Test.ViewModels
{
    [TestClass]
    public class ValidatableViewModelTest : CloseableViewModelTest
    {
        #region Fields

        public const string PropToValidate1 = "PropToValidate1";
        public const string PropToValidate2 = "PropToValidate2";

        #endregion

        #region Properties

        protected ValidatorProvider ValidatorProvider { get; set; }

        #endregion

        #region Test methods

        [TestMethod]
        public void VmShouldValidateSelfByDefault()
        {
            ValidatorProvider.Register<SpyValidator>();
            ValidatableViewModel validatableViewModel = GetValidatableViewModel();
            var validator = (SpyValidator)validatableViewModel.GetValidators().Single(validator1 => validator1 != validatableViewModel.Validator);
            validator.Context.Instance.ShouldEqual(validatableViewModel);
        }

        [TestMethod]
        public void CreateContextShouldCreateContextWithDefaultValuesFromVm()
        {
            var obj = new object();
            ValidatableViewModel viewModel = GetValidatableViewModel();
            IViewModel vm = viewModel;

            IValidatorContext validatorContext = viewModel.CreateContext(obj);
            validatorContext.ServiceProvider.ShouldEqual(vm.IocContainer);
            validatorContext.ValidationMetadata.ShouldEqual(viewModel.Settings.Metadata);
            validatorContext.PropertyMappings.ShouldEqual(viewModel.PropertyMappings);
            validatorContext.IgnoreProperties.ShouldEqual(viewModel.IgnoreProperties);
            validatorContext.Instance.ShouldEqual(obj);
            validatorContext.ServiceProvider.ShouldEqual(vm.IocContainer);
        }

        [TestMethod]
        public void AddNotInitializedValidatorShouldThrowException()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ShouldThrow<InvalidOperationException>(() => viewModel.AddValidator(new SpyValidator()));
        }

        [TestMethod]
        public void RemoveNotInitializedValidatorShouldThrowException()
        {
            var viewModel = GetViewModel<ValidatableViewModel>();
            ShouldThrow<InvalidOperationException>(() => viewModel.RemoveValidator(new SpyValidator()));
        }

        [TestMethod]
        public void AddedValidatorGenericShouldBeInValidators()
        {
            var o = new object();
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator = viewModel.AddValidator<SpyValidator>(o);
            validator.Context.ShouldNotBeNull();
            validator.Context.Instance.ShouldEqual(o);
            viewModel.GetValidators().Single(validator1 => validator1 != viewModel.Validator).ShouldEqual(validator);
        }

        [TestMethod]
        public void AddedValidatorShouldBeInValidators()
        {
            var o = new object();
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator = new SpyValidator();
            validator.Initialize(viewModel.CreateContext(o));
            viewModel.AddValidator(validator);
            validator.Context.ShouldNotBeNull();
            validator.Context.Instance.ShouldEqual(o);
            viewModel.GetValidators().Single(validator1 => validator1 != viewModel.Validator).ShouldEqual(validator);
        }

        [TestMethod]
        public void RemoveValidatorGenericShouldBeRemoveFromValidators()
        {
            var o = new object();
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator = viewModel.AddValidator<SpyValidator>(o);
            viewModel.GetValidators().Single(validator1 => validator1 != viewModel.Validator).ShouldEqual(validator);
            viewModel.RemoveValidator(validator).ShouldBeTrue();
            viewModel.GetValidators().Where(validator1 => validator1 != viewModel.Validator).ShouldBeEmpty();
        }

        [TestMethod]
        public void AddInstanceShouldGetValidatorsFromProviderEmpty()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var instance = new object();
            viewModel.AddInstance(instance);
            viewModel.GetValidators().Where(validator => validator != viewModel.Validator).ShouldBeEmpty();
        }

        [TestMethod]
        public void AddInstanceShouldGetValidatorsFromProviderNotEmpty()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ValidatorProvider.Register<SpyValidator>();
            var instance = new object();
            viewModel.AddInstance(instance);
            viewModel.GetValidators().Single(validator => validator != viewModel.Validator)
                .Context
                .Instance
                .ShouldEqual(instance);
        }

        [TestMethod]
        public void RemoveInstanceShouldRemoveAllValidatorsAssociatedWithInstance()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ValidatorProvider.Register<SpyValidator>();
            var instance = new object();
            var instance2 = new object();
            viewModel.AddInstance(instance);
            viewModel.AddInstance(instance2);
            viewModel.AddValidator<ManualValidator>(instance);
            viewModel.GetValidators().Count.ShouldEqual(4);

            viewModel.RemoveInstance(instance);
            viewModel.GetValidators().Single(validator => validator != viewModel.Validator).Context.Instance.ShouldEqual(instance2);
        }

        [TestMethod]
        public void ValidateShouldRedirectCallToValidators()
        {
            const int count = 10;
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ValidatorProvider.Register<SpyValidator>();

            for (int i = 0; i < count; i++)
            {
                viewModel.AddInstance(new object());
            }
            viewModel.GetValidators().Count.ShouldEqual(count + 1);
            viewModel.ValidateAsync(PropToValidate1);
            viewModel.GetValidators().OfType<SpyValidator>()
                .All(validator => validator.ValidateProperties.Contains(PropToValidate1))
                .ShouldBeTrue();
            viewModel.GetValidators().OfType<SpyValidator>().All(validator => validator.ValidateCount == 1).ShouldBeTrue();
        }

        [TestMethod]
        public void ValidateAllShouldRedirectCallToValidators()
        {
            const int count = 10;
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ValidatorProvider.Register<SpyValidator>();

            for (int i = 0; i < count; i++)
            {
                viewModel.AddInstance(new object());
            }
            viewModel.GetValidators().Count.ShouldEqual(count + 1);
            viewModel.GetValidators().OfType<SpyValidator>().All(validator => validator.ValidateAllCount == 1).ShouldBeTrue();
        }

        [TestMethod]
        public void ValidateInstanceShouldValidateOnlyInstance()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            ValidatorProvider.Register<SpyValidator>();
            var instance = new object();
            var instance2 = new object();
            viewModel.AddInstance(instance);
            viewModel.AddInstance(instance2);
            viewModel.AddValidator<SpyValidator>(instance);
            viewModel.GetValidators().Count.ShouldEqual(4);
            viewModel.GetValidators().OfType<SpyValidator>().ForEach(validator => validator.ValidateAllCount = 0);
            viewModel.ValidateInstanceAsync(instance);

            foreach (SpyValidator result in viewModel.GetValidators().OfType<SpyValidator>())
            {
                if (result.Context.Instance == instance)
                    result.ValidateAllCount.ShouldEqual(1);
                else
                    result.ValidateAllCount.ShouldEqual(0);
            }
        }

        [TestMethod]
        public void IsValidShouldRedirectCallToValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator1 = viewModel.AddValidator<SpyValidator>(new object());
            var validator2 = viewModel.AddValidator<SpyValidator>(new object());

            viewModel.IsValid.ShouldBeTrue();
            validator1.SetErrors(PropToValidate1, PropToValidate1);
            validator2.SetErrors(PropToValidate2, PropToValidate2);
            viewModel.IsValid.ShouldBeFalse();

            validator1.SetErrors(PropToValidate1);
            viewModel.IsValid.ShouldBeFalse();

            validator2.SetErrors(PropToValidate2);
            viewModel.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public void GetErrorsShouldRedirectCallToValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator1 = viewModel.AddValidator<SpyValidator>(new object());
            var validator2 = viewModel.AddValidator<SpyValidator>(new object());

            viewModel.GetErrors(PropToValidate1).ShouldBeEmpty();
            viewModel.GetErrors(PropToValidate2).ShouldBeEmpty();

            validator1.SetErrors(PropToValidate1, PropToValidate1);
            validator1.SetErrors(PropToValidate2, PropToValidate2);
            validator2.SetErrors(PropToValidate1, PropToValidate2);
            validator2.SetErrors(PropToValidate2, PropToValidate1);

            object[] errors = viewModel.GetErrors(PropToValidate1).OfType<object>().ToArray();
            errors.Length.ShouldEqual(2);
            errors.Contains(PropToValidate1).ShouldBeTrue();
            errors.Contains(PropToValidate2).ShouldBeTrue();

            errors = viewModel.GetErrors(PropToValidate2).OfType<object>().ToArray();
            errors.Length.ShouldEqual(2);
            errors.Contains(PropToValidate1).ShouldBeTrue();
            errors.Contains(PropToValidate2).ShouldBeTrue();

            validator1.SetErrors(PropToValidate1);
            validator1.SetErrors(PropToValidate2);

            errors = viewModel.GetErrors(PropToValidate1).OfType<object>().ToArray();
            errors.Length.ShouldEqual(1);
            errors.Contains(PropToValidate2).ShouldBeTrue();

            errors = viewModel.GetErrors(PropToValidate2).OfType<object>().ToArray();
            errors.Length.ShouldEqual(1);
            errors.Contains(PropToValidate1).ShouldBeTrue();

            validator2.SetErrors(PropToValidate1);
            validator2.SetErrors(PropToValidate2);

            viewModel.GetErrors(PropToValidate1).ShouldBeEmpty();
            viewModel.GetErrors(PropToValidate2).ShouldBeEmpty();
        }

        [TestMethod]
        public void GetAllErrorsShouldRedirectCallToValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator1 = viewModel.AddValidator<SpyValidator>(new object());
            var validator2 = viewModel.AddValidator<SpyValidator>(new object());

            viewModel.GetErrors().ShouldBeEmpty();
            viewModel.GetErrors().ShouldBeEmpty();

            validator1.SetErrors(PropToValidate1, PropToValidate1);
            validator1.SetErrors(PropToValidate2, PropToValidate2);
            validator2.SetErrors(PropToValidate1, PropToValidate2);
            validator2.SetErrors(PropToValidate2, PropToValidate1);

            IDictionary<string, IList<object>> errors = viewModel.GetErrors();
            errors.Count.ShouldEqual(2);

            errors[PropToValidate1].Count.ShouldEqual(2);
            errors[PropToValidate1].Contains(PropToValidate1).ShouldBeTrue();
            errors[PropToValidate1].Contains(PropToValidate2).ShouldBeTrue();

            errors[PropToValidate2].Count.ShouldEqual(2);
            errors[PropToValidate2].Contains(PropToValidate1).ShouldBeTrue();
            errors[PropToValidate2].Contains(PropToValidate2).ShouldBeTrue();

            validator1.SetErrors(PropToValidate1);
            validator1.SetErrors(PropToValidate2);

            errors = viewModel.GetErrors();
            errors.Count.ShouldEqual(2);

            errors[PropToValidate1].Count.ShouldEqual(1);
            errors[PropToValidate1].Contains(PropToValidate2).ShouldBeTrue();

            errors[PropToValidate2].Count.ShouldEqual(1);
            errors[PropToValidate2].Contains(PropToValidate1).ShouldBeTrue();

            validator2.SetErrors(PropToValidate1);
            validator2.SetErrors(PropToValidate2);

            viewModel.GetErrors().ShouldBeEmpty();
            viewModel.GetErrors().ShouldBeEmpty();
        }

        [TestMethod]
        public void ClearErrorsPropertyShouldRedirectCallToValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator1 = viewModel.AddValidator<SpyValidator>(new object());
            var validator2 = viewModel.AddValidator<SpyValidator>(new object());

            viewModel.ClearErrors(PropToValidate1);
            validator1.ClearPropertyErrorsCount.ShouldEqual(1);
            validator2.ClearPropertyErrorsCount.ShouldEqual(1);
        }


        [TestMethod]
        public void ClearErrorsShouldRedirectCallToValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            var validator1 = viewModel.AddValidator<SpyValidator>(new object());
            var validator2 = viewModel.AddValidator<SpyValidator>(new object());

            viewModel.ClearErrors();
            validator1.ClearAllErrorsCount.ShouldEqual(1);
            validator2.ClearAllErrorsCount.ShouldEqual(1);
        }

#if HASDATAERROR
        [TestMethod]
        public void DataErrorInfoShouldGetErrorsFromValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            IDataErrorInfo dataErrorInfo = viewModel;

            var validator = viewModel.AddValidator<SpyValidator>(viewModel);
            IDataErrorInfo validatorErrorInfo = validator;

            validator.SetErrors(PropToValidate1, PropToValidate1);
            viewModel.OnPropertyChanged(PropToValidate1, ExecutionType.None);
            string error = dataErrorInfo[PropToValidate1];
            validator.ValidateCount.ShouldEqual(1);
            validator.ValidateProperties.Contains(PropToValidate1).ShouldBeTrue();
            error.ShouldEqual(PropToValidate1);

            validator.SetErrors(PropToValidate1);
            viewModel.OnPropertyChanged(PropToValidate1, ExecutionType.None);
            error = validatorErrorInfo[PropToValidate1];
            validator.ValidateCount.ShouldEqual(2);
            error.ShouldBeNull();
        }
#endif
        [TestMethod]
        public void NotifyDataErrorInfoShouldGetErrorsFromValidators()
        {
            ValidatableViewModel viewModel = GetValidatableViewModel();
            INotifyDataErrorInfo notifyDataError = viewModel;

            var validator = viewModel.AddValidator<SpyValidator>(new object());

            notifyDataError.HasErrors.ShouldBeFalse();
            validator.SetErrors(PropToValidate1, PropToValidate1, PropToValidate2);
            notifyDataError.HasErrors.ShouldBeTrue();

            string[] errors = notifyDataError.GetErrors(PropToValidate1).OfType<string>().ToArray();
            errors.Length.ShouldEqual(2);
            errors.Contains(PropToValidate1).ShouldBeTrue();
            errors.Contains(PropToValidate2).ShouldBeTrue();

            validator.SetErrors(PropToValidate1);
            notifyDataError.GetErrors(PropToValidate1).ShouldBeEmpty();
            notifyDataError.HasErrors.ShouldBeFalse();
        }

        [TestMethod]
        public void ValidateMethodShouldCallNotifyEvent()
        {
            ThreadManager.ImmediateInvokeAsync = true;
            ThreadManager.ImmediateInvokeOnUiThreadAsync = true;
            ThreadManager.ImmediateInvokeOnUiThread = true;

            int countInvoke = 0;
            ValidatableViewModel viewModel = GetValidatableViewModel();
            INotifyDataErrorInfo notifyDataError = viewModel;
            notifyDataError.ErrorsChanged += (sender, args) =>
            {
                args.PropertyName.ShouldEqual(PropToValidate1);
                countInvoke++;
            };
            var validator = viewModel.AddValidator<SpyValidator>(new object());

            notifyDataError.HasErrors.ShouldBeFalse();
            validator.SetErrors(PropToValidate1, PropToValidate1);
            countInvoke.ShouldEqual(1);
            validator.SetErrors(PropToValidate1, PropToValidate2);
            countInvoke.ShouldEqual(2);
            notifyDataError.HasErrors.ShouldBeTrue();
        }

        #endregion

        #region Methods

        protected virtual ValidatableViewModel GetValidatableViewModel()
        {
            return GetViewModel<ValidatableViewModel>();
        }

        #endregion

        #region Overrides of TestBase

        protected override object GetFunc(Type type, string s, IIocParameter[] arg3)
        {
            if (type == typeof(IValidatorProvider))
                return ValidatorProvider;
            return base.GetFunc(type, s, arg3);
        }

        protected override void OnInit()
        {
            ValidatorProvider = new ValidatorProvider();
        }

        #endregion

        #region Overrides of CloseableViewModelTest

        protected override ICloseableViewModel GetCloseableViewModelInternal()
        {
            return GetValidatableViewModel();
        }

        #endregion
    }
}
