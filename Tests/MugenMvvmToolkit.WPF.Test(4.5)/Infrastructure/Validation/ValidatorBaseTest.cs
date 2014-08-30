using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.Models.Validation;
using MugenMvvmToolkit.Test.TestModels;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Validation
{
    [TestClass]
    public class ValidatorBaseTest : TestBase
    {
        #region Fields

        public const string ValidatorError = "validatorError";
        public static readonly string[] ValidatorErrors = { ValidatorError };
        public const string PropertyToValidate = "PropertyToValidate";
        public const string PropertyToMap = "PropertyToMap";

        #endregion

        #region Test methods

        [TestMethod]
        public void NotInitializedValidatorShouldThrowErrorTest()
        {
            ValidatorBase validator = GetValidator();
            ShouldThrow<InvalidOperationException>(() => validator.GetErrors(PropertyToValidate));
        }

        [TestMethod]
        public void DoubleInitializedValidatorShouldThrowErrorTest()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            ShouldThrow<InvalidOperationException>(
                () => validator.Initialize(new ValidatorContext(new object(), GetServiceProvider())));
        }

        [TestMethod]
        public void CloneShouldCreateNewValidator()
        {
            ValidatorBase validator = GetValidator();
            validator.Context.ShouldBeNull();
            IValidator clone = validator.Clone();
            validator.ShouldNotEqual(clone);
            clone.Context.ShouldBeNull();

            clone.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            clone.Context.ShouldNotBeNull();
            IValidator clone2 = clone.Clone();
            clone.ShouldNotEqual(clone2);
            clone2.Context.ShouldBeNull();
        }

        [TestMethod]
        public void UpdateErrorsShouldNotifyListeners()
        {
            bool isAsync = false;
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            var spyHandler = new SpyHandler
            {
                HandleDelegate = (o, o1) =>
                {
                    o.ShouldEqual(validator);
                    ((DataErrorsChangedMessage)o1).PropertyName.ShouldEqual(PropertyToValidate);
                    ((DataErrorsChangedMessage)o1).IsAsyncValidate.ShouldEqual(isAsync);
                }
            };
            validator.Subscribe(spyHandler).ShouldBeTrue();

            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, isAsync);
            isAsync = true;
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, isAsync);
            spyHandler.HandleCount.ShouldEqual(2);
        }

        [TestMethod]
        public void UpdateErrorsShouldRaiseEvent()
        {
            bool isInvoked = false;
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.ErrorsChanged += (sender, args) =>
            {
                sender.ShouldEqual(validator);
                args.PropertyName.ShouldEqual(PropertyToValidate);
                isInvoked = true;
            };
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            isInvoked.ShouldBeTrue();
        }

        [TestMethod]
        public void IsValidShouldBeFalseIfValidatorHasErrors()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.IsValid.ShouldBeTrue();
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.IsValid.ShouldBeFalse();
            validator.UpdateErrors(PropertyToValidate, null, false);
            validator.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public void GetErrorsShouldReturnValidatorErrors()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.GetErrors(PropertyToValidate).ShouldBeEmpty();

            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.GetErrors(PropertyToValidate).OfType<object>().Single().ShouldEqual(ValidatorError);

            validator.UpdateErrors(PropertyToValidate, new object[] { ValidatorError, ValidatorError }, false);
            var errors = validator.GetErrors(PropertyToValidate).OfType<object>().ToArray();
            errors.Length.ShouldEqual(2);
            errors.All(o => Equals(o, ValidatorError)).ShouldBeTrue();
        }

        [TestMethod]
        public void GetAllErrorsShouldReturnAllValidatorErrors()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));

            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.UpdateErrors(PropertyToMap, ValidatorErrors, false);
            var allErrors = validator.GetErrors();
            allErrors.Count.ShouldEqual(2);
            allErrors[PropertyToValidate].Single().ShouldEqual(ValidatorError);
            allErrors[PropertyToMap].Single().ShouldEqual(ValidatorError);
        }

        [TestMethod]
        public void ValidatorShouldUseIgnorePropertiesCollection()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), null, new List<string> { PropertyToValidate }, null,
                GetServiceProvider()));
            validator.IsValid.ShouldBeTrue();
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.IsValid.ShouldBeTrue();
            validator.GetErrors(PropertyToValidate).ShouldBeEmpty();
        }

        [TestMethod]
        public void ValidatorShouldUsePropertyMappings()
        {
            ValidatorBase validator = GetValidator();
            var dictionary = new Dictionary<string, ICollection<string>>
            {
                {
                    PropertyToValidate,
                    new List<string> {PropertyToMap}
                }
            };
            validator.Initialize(new ValidatorContext(new object(), dictionary, null, null, GetServiceProvider()));
            validator.IsValid.ShouldBeTrue();
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.IsValid.ShouldBeFalse();
            validator.GetErrors(PropertyToValidate).ShouldBeEmpty();

            string[] errors = validator.GetErrors(PropertyToMap).OfType<string>().ToArray();
            errors.Length.ShouldEqual(1);
            errors.Contains(ValidatorError).ShouldBeTrue();

            validator.UpdateErrors(PropertyToMap, null, false);
            validator.GetErrors(PropertyToMap).ShouldBeEmpty();
            validator.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public virtual void ValidatorShouldClearOldErrorsValidateAll()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);

            validator.ValidateAsync().Wait();
            if (validator is ManualValidator)
            {
                var allErrors = validator.GetErrors();
                allErrors.Count.ShouldEqual(1);
                allErrors[PropertyToValidate].Single().ShouldEqual(ValidatorErrors[0]);
            }
            else
                validator.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public void NotifyDataErrorInfoTest()
        {
            ValidatorBase validator = GetValidator();
            INotifyDataErrorInfo notifyDataError = validator;

            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            notifyDataError.HasErrors.ShouldBeFalse();
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            notifyDataError.HasErrors.ShouldBeTrue();

            string[] errors = notifyDataError
                .GetErrors(PropertyToValidate)
                .OfType<string>()
                .ToArray();
            errors.Length.ShouldEqual(1);
            errors.Contains(ValidatorError).ShouldBeTrue();

            validator.UpdateErrors(PropertyToValidate, null, false);
            notifyDataError.GetErrors(PropertyToValidate).ShouldBeEmpty();
            notifyDataError.HasErrors.ShouldBeFalse();
        }

        [TestMethod]
        public void WhenPropertyChangedInEntityValidatorShouldCallValidateMethod()
        {
            var modelToValidate = new ValidatableModel();
            var validator = new SpyValidator();
            validator.ValidateOnPropertyChanged = true;
            validator.Initialize(new ValidatorContext(modelToValidate, GetServiceProvider()));

            validator.ValidateCount.ShouldEqual(0);
            modelToValidate.OnPropertyChanged(PropertyToValidate, ExecutionMode.None);
            validator.ValidateCount.ShouldEqual(1);
            validator.ValidateProperties.ShouldContain(PropertyToValidate);
            validator.ValidateAllCount.ShouldEqual(0);
        }

        [TestMethod]
        public void WhenPropertyChangedInEntityValidatorShouldNotCallValidateMethodFalse()
        {
            var modelToValidate = new ValidatableModel();
            var validator = new SpyValidator();
            validator.ValidateOnPropertyChanged = false;
            validator.Initialize(new ValidatorContext(modelToValidate, GetServiceProvider()));

            validator.ValidateCount.ShouldEqual(0);
            modelToValidate.OnPropertyChanged(PropertyToValidate, ExecutionMode.None);
            validator.ValidateCount.ShouldEqual(0);
            validator.ValidateAllCount.ShouldEqual(0);
        }

        [TestMethod]
        public void ValidatorShouldClearPropertyErrors()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.UpdateErrors(PropertyToMap, ValidatorErrors, false);

            validator.GetErrors(PropertyToValidate).OfType<object>().Single().ShouldEqual(ValidatorError);
            validator.GetErrors(PropertyToMap).OfType<object>().Single().ShouldEqual(ValidatorError);

            validator.ClearErrors(PropertyToValidate);
            validator.GetErrors(PropertyToValidate).OfType<object>().ShouldBeEmpty();
            validator.GetErrors(PropertyToMap).OfType<object>().Single().ShouldEqual(ValidatorError);

            validator.ClearErrors(PropertyToMap);
            validator.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public void ValidatorShouldUpdateErrors()
        {
            ValidatorBase validator = GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            validator.UpdateErrors(PropertyToMap, ValidatorErrors, false);

            validator.GetErrors(PropertyToValidate).OfType<object>().Single().ShouldEqual(ValidatorError);
            validator.GetErrors(PropertyToMap).OfType<object>().Single().ShouldEqual(ValidatorError);

            validator.ClearErrors();
            validator.GetErrors(PropertyToValidate).OfType<object>().ShouldBeEmpty();
            validator.GetErrors(PropertyToMap).OfType<object>().ShouldBeEmpty();
            validator.IsValid.ShouldBeTrue();
        }

        [TestMethod]
        public void ValidateMethodShouldRaiseErrorsChangedEvent()
        {
            int countInvoke = 0;
            ValidatorBase validator = GetValidator();
            INotifyDataErrorInfo notifyDataError = validator;
            notifyDataError.ErrorsChanged += (sender, args) =>
            {
                args.PropertyName.ShouldEqual(PropertyToValidate);
                countInvoke++;
            };

            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            notifyDataError.HasErrors.ShouldBeFalse();
            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            countInvoke.ShouldEqual(1);
            notifyDataError.HasErrors.ShouldBeTrue();

            if (validator is ManualValidator)
                validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            else
                validator.ValidateAsync(PropertyToValidate);
            countInvoke.ShouldEqual(2);
        }

#if HASDATAERROR
        [TestMethod]
        public void DataErrorTest()
        {
            ValidatorBase validator = GetValidator();
            IDataErrorInfo validatorDataError = validator;
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.IsValid().ShouldBeTrue();

            validator.UpdateErrors(PropertyToValidate, ValidatorErrors, false);
            string error = validatorDataError[PropertyToValidate];
            error.ShouldEqual(ValidatorError);
            validator.IsValid().ShouldBeFalse();

            validator.UpdateErrors(PropertyToValidate, null, false);
            error = validatorDataError[PropertyToValidate];
            error.ShouldBeNull();
            validator.IsValid().ShouldBeTrue();
        }
#endif

        #endregion

        #region Methods

        protected virtual IServiceProvider GetServiceProvider()
        {
            return IocContainer;
        }

        protected virtual ValidatorBase GetValidator()
        {
            return new ManualValidator();
        }

        #endregion
    }
}