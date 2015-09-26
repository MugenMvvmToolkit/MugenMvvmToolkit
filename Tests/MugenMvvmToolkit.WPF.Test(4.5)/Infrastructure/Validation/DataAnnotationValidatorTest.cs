using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Models.Validation;
using MugenMvvmToolkit.Test.TestInfrastructure;
using MugenMvvmToolkit.Test.TestModels;
using Should;

#if !WPF
namespace System.ComponentModel.DataAnnotations
{

    public interface IValidatableObject
    {
        IEnumerable<ValidationResult> Validate(ValidationContext validationContext);
    }
}
#endif

namespace MugenMvvmToolkit.Test.Infrastructure.Validation
{
    [TestClass]
    public class DataAnnotationValidatorTest : ValidatorBaseTest
    {
        #region Nested types

        public sealed class SpyValidationClass : System.ComponentModel.DataAnnotations.IValidatableObject
        {
            #region Properties

            [SpyValidation]
            public string SpyProperty { get; set; }

            public Func<ValidationContext, IEnumerable<ValidationResult>> ValidateSystem { get; set; }

            #endregion

            #region Implementation of IValidatableObject

            IEnumerable<ValidationResult> System.ComponentModel.DataAnnotations.IValidatableObject.Validate(
                ValidationContext validationContext)
            {
                return ValidateSystem(validationContext);
            }

            #endregion
        }

        [System.ComponentModel.DataAnnotations.MetadataType(typeof(MetaSystem))]
        public sealed class MetadataValidationClassSystem
        {
            public string SpyProperty { get; set; }
        }

        public sealed class MetaSystem
        {
            [SpyValidation, Required]
            public string SpyProperty { get; set; }
        }

        [MugenMvvmToolkit.Attributes.MetadataType(typeof(MetaToolkit))]
        public sealed class MetadataValidationClassToolkit
        {
            public string SpyProperty { get; set; }
        }

        public sealed class MetaToolkit
        {
            [SpyValidation, Required]
            public string SpyProperty { get; set; }
        }

        [MugenMvvmToolkit.Attributes.MetadataType(typeof(MetaToolkit))]
        public sealed class InvalidaMetadataClass
        {
            public string Prop { get; set; }
        }

        public sealed class InvalidaMetadata
        {
            [SpyValidation]
            public string Prop1 { get; set; }
        }

        #endregion

        #region Fields

        public const string DisplayName = "DisplayName";
        public const string SpyPropertyName = "SpyProperty";

        #endregion

        #region Methods

        [TestMethod]
        public void ValidatorShouldFindAllElements()
        {
            const int count = 2;
            const int objectCount = 1;

            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            var validationClass = new SpyValidationClass();
            var elements = DataAnnotationValidatior.GetValidationElements(validationClass);
            elements.Count.ShouldEqual(count);
            elements[string.Empty].Count.ShouldEqual(objectCount);
            elements[SpyPropertyName].Count.ShouldEqual(1);
        }

        [TestMethod]
        public void ValidatorShouldThrowExceptionIfMetadataIsInvalid()
        {
            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            var validationClass = new InvalidaMetadataClass();
            ShouldThrow<MissingMemberException>(() => DataAnnotationValidatior.GetValidationElements(validationClass));
        }

        [TestMethod]
        public void ValidatorShouldValidateItemUsingSystemValidatableObject()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            DisplayNameProvider.GetNameDelegate = info =>
            {
#if NETFX_CORE
                info.ShouldEqual(typeof(SpyValidationClass).GetTypeInfo());
#else
                info.ShouldEqual(typeof(SpyValidationClass));
#endif
                return DisplayName;
            };

            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            DataAnnotationValidatior.ElementsCache.Clear();
            DataAnnotationValidatior.DisplayNameProvider = null;
            var validationClass = new SpyValidationClass
            {
                ValidateSystem = context =>
                {
                    context.DisplayName.ShouldEqual(DisplayName);
                    context.Items[DataAnnotationValidatior.ServiceProviderKey].ShouldEqual(IocContainer);
                    context.MemberName.ShouldBeNull();
                    return new[] { result };
                }
            };

            var validationContext = new DataAnnotationValidatior.ValidationContext(validationClass, IocContainer, dictionary);
            var elements = DataAnnotationValidatior.GetValidationElements(validationClass);
            var validationResults = new List<object>();
            elements[string.Empty].ForEach(element => validationResults.AddRange(element.Validate(validationContext)));

            var validationResult = validationResults.OfType<ValidationResult>().Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidatorShouldValidateUsingDataAnnotationAttributes()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            var propertyInfo = typeof(SpyValidationClass).GetProperty(SpyPropertyName);
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(propertyInfo);
                return DisplayName + propertyInfo.Name;
            };
            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            DataAnnotationValidatior.ElementsCache.Clear();
            DataAnnotationValidatior.DisplayNameProvider = null;
            var validationClass = new SpyValidationClass { SpyProperty = "Test" };
            var validationContext = new DataAnnotationValidatior.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DataAnnotationValidatior.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };
            var validationResult = DataAnnotationValidatior.GetValidationElements(validationClass)[SpyPropertyName]
                .Single()
                .Validate(validationContext)
                .OfType<ValidationResult>()
                .Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidatorShouldValidateUsingDataAnnotationAttributesWithSystemMetadata()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            var propertyInfo = typeof(MetadataValidationClassSystem).GetProperty(SpyPropertyName);
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(propertyInfo);
                return DisplayName + propertyInfo.Name;
            };
            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            DataAnnotationValidatior.ElementsCache.Clear();
            DataAnnotationValidatior.DisplayNameProvider = null;
            var validationClass = new MetadataValidationClassSystem { SpyProperty = "Test" };
            var validationContext = new DataAnnotationValidatior.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DataAnnotationValidatior.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };
            var validationElements = DataAnnotationValidatior.GetValidationElements(validationClass)[SpyPropertyName];
            validationElements.Count.ShouldEqual(2);

            var validationResults = validationElements
                .SelectMany(element => element.Validate(validationContext))
                .ToArray();
            validationResults.Length.ShouldEqual(1);
            var validationResult = validationResults.OfType<ValidationResult>().Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ProviderShouldValidateUsingDataAnnotationAttributesWithToolkitMetadata()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            var propertyInfo = typeof(MetadataValidationClassToolkit).GetProperty(SpyPropertyName);
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(propertyInfo);
                return DisplayName + propertyInfo.Name;
            };
            DataAnnotationValidatior.GetValidationElementsDelegate = null;
            DataAnnotationValidatior.ElementsCache.Clear();
            DataAnnotationValidatior.DisplayNameProvider = null;
            var validationClass = new MetadataValidationClassToolkit { SpyProperty = "Test" };
            var validationContext = new DataAnnotationValidatior.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DataAnnotationValidatior.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };

            var validationElements = DataAnnotationValidatior.GetValidationElements(validationClass)[SpyPropertyName];
            validationElements.Count.ShouldEqual(2);

            var validationResults = validationElements
                .SelectMany(element => element.Validate(validationContext))
                .ToArray();
            validationResults.Length.ShouldEqual(1);
            var validationResult = validationResults.OfType<ValidationResult>().Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ValidateShouldUseOnlyValidationElementsWithPropertyName()
        {
            var el1ValidationResults = new object[]
            {
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
                new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
            };
            var el2ValidationResults = new object[]
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
            DataAnnotationValidatior.GetValidationElementsDelegate =
                o => new Dictionary<string, List<DataAnnotationValidatior.IValidationElement>>
                {
                    {PropertyToValidate, new List<DataAnnotationValidatior.IValidationElement> {el1}},
                    {PropertyToMap, new List<DataAnnotationValidatior.IValidationElement> {el2}}
                };
            var validator = (DataAnnotationValidatior)GetValidator();
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
            var el1ValidationResults = new object[]
               {
                   new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
                   new ValidationResult(PropertyToValidate, new[] {PropertyToValidate}),
               };
            var el2ValidationResults = new object[]
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
            DataAnnotationValidatior.GetValidationElementsDelegate =
                o => new Dictionary<string, List<DataAnnotationValidatior.IValidationElement>>
                {
                    {PropertyToValidate, new List<DataAnnotationValidatior.IValidationElement> {el1}},
                    {PropertyToMap, new List<DataAnnotationValidatior.IValidationElement> {el2}}
                };
            var validator = (DataAnnotationValidatior)GetValidator();
            validator.Initialize(new ValidatorContext(item, GetServiceProvider())).ShouldBeTrue();

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
            return new DataAnnotationValidatior();
        }

        protected override void OnInit()
        {
            base.OnInit();
            DataAnnotationValidatior.GetValidationElementsDelegate = o => new Dictionary<string, List<DataAnnotationValidatior.IValidationElement>>
                {
                    {"Test", new List<DataAnnotationValidatior.IValidationElement> {new ValidationElementMock()}},
                    {PropertyToValidate, new List<DataAnnotationValidatior.IValidationElement> {new ValidationElementMock()}}
                };
        }

        #endregion
    }
}
