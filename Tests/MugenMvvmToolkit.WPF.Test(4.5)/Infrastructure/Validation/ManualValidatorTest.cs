using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Validation;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Validation
{
    [TestClass]
    public class ManualValidatorTest : ValidatorBaseTest
    {
        #region Methods

        [TestMethod]
        public void SetErrorsShouldUpdateErrorsInValidator()
        {
            var validator = (ManualValidator)GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.IsValid.ShouldBeTrue();

            validator.SetErrors(PropertyToValidate, ValidatorError);
            validator.IsValid.ShouldBeFalse();

            validator.ValidateAsync(PropertyToValidate).Wait();
            validator.GetErrors(PropertyToValidate).OfType<object>().Single().ShouldEqual(ValidatorError);

            var allErrors = validator.GetErrors();
            allErrors.Count.ShouldEqual(1);
            allErrors[PropertyToValidate].Single().ShouldEqual(ValidatorError);
        }

        [TestMethod]
        public void SetErrorsGenericShouldUpdateErrorsInValidator()
        {
            const string propName = "Entity";
            var validator = (ManualValidator)GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.IsValid.ShouldBeTrue();

            validator.SetErrors<EntityStateEntry>(entry => entry.Entity, ValidatorError);
            validator.IsValid.ShouldBeFalse();

            validator.ValidateAsync(propName).Wait();
            validator.GetErrors(propName).OfType<object>().Single().ShouldEqual(ValidatorError);

            var allErrors = validator.GetErrors();
            allErrors.Count.ShouldEqual(1);
            allErrors[propName].Single().ShouldEqual(ValidatorError);
        }

        #endregion

        #region Overrides of ValidatorBaseTest

        protected override ValidatorBase GetValidator()
        {
            return new ManualValidator();
        }

        #endregion
    }
}