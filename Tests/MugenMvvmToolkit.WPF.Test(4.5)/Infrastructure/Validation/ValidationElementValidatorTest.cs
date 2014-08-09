using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.Validation;
using MugenMvvmToolkit.Test.TestInfrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure.Validation
{
    [TestClass]
    public class ValidationElementValidatorTest : ValidatorBaseTest
    {
        #region Properties

        protected ValidationElementProviderMock ValidationElementProvider { get; set; }

        #endregion

        #region Methods

        [TestMethod]
        public void ValidatorShouldGetProviderFromServiceProvider()
        {
            var validator = (ValidationElementValidator)GetValidator();
            validator.Initialize(new ValidatorContext(new object(), GetServiceProvider()));
            validator.ValidationElementProvider.ShouldEqual(ValidationElementProvider);
        }

        [TestMethod]
        public void ValidateShouldUseOnlyValidationElementsWithPropertyName()
        {
            var el1ValidationResults = new IValidationResult[]
            {
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
            };
            var el2ValidationResults = new IValidationResult[]
            {
                new ValidationResult(PropertyToMap, new[] {PropertyToMap}),
                new ValidationResult(PropertyToMap, new[] {PropertyToMap}),
            };
            bool el1IsInvoked = false;
            bool el2IsInvoked = false;
            var item = new object();
            var el1 = new ValidationElementMock();
            var el2 = new ValidationElementMock();
            el1.Validate = context =>
            {
                context.ObjectInstance.ShouldEqual(item);
                context.ServiceProvider.ShouldEqual(IocContainer);
                el1IsInvoked = true;
                return el1ValidationResults;
            };
            el2.Validate = context =>
            {
                el2IsInvoked = true;
                return el2ValidationResults;
            };
            ValidationElementProvider.GetValidationElements =
                o => new Dictionary<string, IList<IValidationElement>>
                {
                    {PropertyToValidate, new IValidationElement[] {el1}},
                    {PropertyToMap, new IValidationElement[] {el2}}
                };
            var validator = (ValidationElementValidator)GetValidator();
            validator.Initialize(new ValidatorContext(item, GetServiceProvider()));

            validator.ValidateAsync(PropertyToValidate).Wait();
            el1IsInvoked.ShouldBeTrue();
            el2IsInvoked.ShouldBeFalse();

            var allErrors = validator.GetErrors();
            allErrors.Count.ShouldEqual(1);
            allErrors[PropertyToValidate].ShouldEqual(el1ValidationResults);
        }

        [TestMethod]
        public void ValidateAllShouldUseAllValidationElements()
        {
            var el1ValidationResults = new IValidationResult[]
            {
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
            };
            var el2ValidationResults = new IValidationResult[]
            {
                new ValidationResult(PropertyToMap, new[] {PropertyToMap}),
                new ValidationResult(PropertyToMap, new[] {PropertyToMap}),
            };
            bool el1IsInvoked = false;
            bool el2IsInvoked = false;
            var item = new object();
            var el1 = new ValidationElementMock();
            var el2 = new ValidationElementMock();
            el1.Validate = context =>
            {
                context.ObjectInstance.ShouldEqual(item);
                context.ServiceProvider.ShouldEqual(IocContainer);
                el1IsInvoked = true;
                return el1ValidationResults;
            };
            el2.Validate = context =>
            {
                el2IsInvoked = true;
                return el2ValidationResults;
            };
            ValidationElementProvider.GetValidationElements =
                o => new Dictionary<string, IList<IValidationElement>>
                {
                    {PropertyToValidate, new IValidationElement[] {el1}},
                    {PropertyToMap, new IValidationElement[] {el2}}
                };
            var validator = (ValidationElementValidator)GetValidator();
            validator.Initialize(new ValidatorContext(item, GetServiceProvider()));

            validator.ValidateAsync().Wait();
            el1IsInvoked.ShouldBeTrue();
            el2IsInvoked.ShouldBeTrue();

            var allErrors = validator.GetErrors();
            allErrors.Count.ShouldEqual(2);
            allErrors[PropertyToValidate].ShouldEqual(el1ValidationResults);
            allErrors[PropertyToMap].ShouldEqual(el2ValidationResults);
        }

        #endregion

        #region Overrides of ValidatorBaseTest

        protected override ValidatorBase GetValidator()
        {
            return new ValidationElementValidator();
        }

        protected override object GetFunc(Type type, string s, IIocParameter[] arg3)
        {
            if (type == typeof(IValidationElementProvider))
                return ValidationElementProvider;
            return base.GetFunc(type, s, arg3);
        }

        protected override void OnInit()
        {
            base.OnInit();
            ValidationElementProvider = new ValidationElementProviderMock
            {
                GetValidationElements = o => new Dictionary<string, IList<IValidationElement>>
                {
                    {"Test", new IValidationElement[]{new ValidationElementMock() }}
                }
            };
            CanBeResolvedTypes.Add(typeof(IValidationElementProvider));
        }

        #endregion
    }
}