using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Test.TestModels;
using MugenMvvmToolkit.ViewModels;
using Should;
using IValidatableObject = MugenMvvmToolkit.Interfaces.Validation.IValidatableObject;

namespace MugenMvvmToolkit.Test.Infrastructure.Validation
{
    [TestClass]
    public class DynamicDataAnnotationsElementProviderTest : TestBase
    {
        #region Nested types

        public sealed class SpyValidationClass : IValidatableObject
#if WPF
, System.ComponentModel.DataAnnotations.IValidatableObject
#endif

        {
            #region Properties

            [SpyValidation]
            public string SpyProperty { get; set; }

#if WPF
            public Func<ValidationContext, IEnumerable<ValidationResult>> ValidateSystem { get; set; }
#endif
            public Func<IValidationContext, IEnumerable<IValidationResult>> Validate { get; set; }

            #endregion

            #region Implementation of IValidatableObject

            /// <summary>
            ///     Determines whether the specified object is valid.
            /// </summary>
            /// <returns>
            ///     A collection that holds failed-validation information.
            /// </returns>
            /// <param name="validationContext">The validation context.</param>
            IEnumerable<IValidationResult> IValidatableObject.Validate(IValidationContext validationContext)
            {
                return Validate(validationContext);
            }

            #endregion

            #region Implementation of IValidatableObject

#if WPF
            /// <summary>
            ///     Determines whether the specified object is valid.
            /// </summary>
            /// <returns>
            ///     A collection that holds failed-validation information.
            /// </returns>
            /// <param name="validationContext">The validation context.</param>
            IEnumerable<ValidationResult> System.ComponentModel.DataAnnotations.IValidatableObject.Validate(
                ValidationContext validationContext)
            {
                return ValidateSystem(validationContext);
            }
#endif


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
        public void ProviderShouldFindAllElements()
        {            
#if WPF
            const int count = 2;
            const int objectCount = 2;
#else
            const int count = 2;
            const int objectCount = 1;
#endif
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new SpyValidationClass();
            var elements = provider.GetValidationElements(validationClass);
            elements.Count.ShouldEqual(count);
            elements[string.Empty].Count.ShouldEqual(objectCount);
            elements[SpyPropertyName].Count.ShouldEqual(1);
        }

        [TestMethod]
        public void ProviderShouldThrowExceptionIfMetadataIsInvalid()
        {
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new InvalidaMetadataClass();
            ShouldThrow<MissingMemberException>(() => provider.GetValidationElements(validationClass));
        }

#if WPF
        [TestMethod]
        public void ProviderShouldValidateItemUsingSystemValidatableObject()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(typeof(SpyValidationClass));
                return DisplayName;
            };
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new SpyValidationClass
            {
                ValidateSystem = context =>
                {
                    context.DisplayName.ShouldEqual(DisplayName);
                    context.Items[DynamicDataAnnotationsElementProvider.ServiceProviderKey].ShouldEqual(IocContainer);
                    context.MemberName.ShouldBeNull();
                    return new[] { result };
                },
                Validate = context => Enumerable.Empty<IValidationResult>()
            };

            var validationContext = new MugenMvvmToolkit.Models.Validation.ValidationContext(validationClass, IocContainer, dictionary);
            var elements = provider.GetValidationElements(validationClass);
            var validationResults = new List<IValidationResult>();
            elements[string.Empty].ForEach(element => validationResults.AddRange(element.Validate(validationContext)));

            var validationResult = validationResults.Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }
#endif


        [TestMethod]
        public void ProviderShouldValidateItemUsingToolkitValidatableObject()
        {
#if NETFX_CORE
            var target = typeof (SpyValidationClass).GetTypeInfo();
#else
            var target = typeof(SpyValidationClass);
#endif
            var result = new MugenMvvmToolkit.Models.Validation.ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(target);
                return DisplayName;
            };
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new SpyValidationClass
            {
                Validate = context =>
                {
                    context.DisplayName.ShouldEqual(DisplayName);
                    context.Items[DynamicDataAnnotationsElementProvider.ServiceProviderKey].ShouldEqual(IocContainer);
                    context.MemberName.ShouldBeNull();
                    return new[] { result };
                },
#if WPF
                ValidateSystem = context => Enumerable.Empty<ValidationResult>()
#endif
            };

            var validationContext = new MugenMvvmToolkit.Models.Validation.ValidationContext(validationClass, IocContainer, dictionary);
            var elements = provider.GetValidationElements(validationClass);
            var validationResults = new List<IValidationResult>();
            elements[string.Empty].ForEach(element => validationResults.AddRange(element.Validate(validationContext)));

            var validationResult = validationResults.Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ProviderShouldValidateUsingDataAnnotationAttributes()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            var propertyInfo = typeof(SpyValidationClass).GetProperty(SpyPropertyName);
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(propertyInfo);
                return DisplayName + propertyInfo.Name;
            };
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new SpyValidationClass { SpyProperty = "Test" };
            var validationContext = new MugenMvvmToolkit.Models.Validation.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DynamicDataAnnotationsElementProvider.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };
            var validationResult = provider.GetValidationElements(validationClass)[SpyPropertyName]
                .Single()
                .Validate(validationContext)
                .Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        [TestMethod]
        public void ProviderShouldValidateUsingDataAnnotationAttributesWithSystemMetadata()
        {
            var result = new ValidationResult("error", new[] { DisplayName });
            var dictionary = new Dictionary<object, object>();
            var propertyInfo = typeof(MetadataValidationClassSystem).GetProperty(SpyPropertyName);
            DisplayNameProvider.GetNameDelegate = info =>
            {
                info.ShouldEqual(propertyInfo);
                return DisplayName + propertyInfo.Name;
            };
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new MetadataValidationClassSystem { SpyProperty = "Test" };
            var validationContext = new MugenMvvmToolkit.Models.Validation.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DynamicDataAnnotationsElementProvider.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };
            var validationElements = provider.GetValidationElements(validationClass)[SpyPropertyName];
            validationElements.Count.ShouldEqual(2);

            var validationResults = validationElements
                .SelectMany(element => element.Validate(validationContext))
                .ToArray();
            validationResults.Length.ShouldEqual(1);
            var validationResult = validationResults.Single();
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
            var provider = new DynamicDataAnnotationsElementProvider(DisplayNameProvider);
            var validationClass = new MetadataValidationClassToolkit { SpyProperty = "Test" };
            var validationContext = new MugenMvvmToolkit.Models.Validation.ValidationContext(validationClass, IocContainer, dictionary);
            SpyValidationAttribute.IsValidDelegate = (o, context) =>
            {
                o.ShouldEqual(validationClass.SpyProperty);
                context.DisplayName.ShouldEqual(DisplayName + propertyInfo.Name);
                context.Items[DynamicDataAnnotationsElementProvider.ServiceProviderKey].ShouldEqual(IocContainer);
                context.MemberName.ShouldEqual(SpyPropertyName);
                return result;
            };

            var validationElements = provider.GetValidationElements(validationClass)[SpyPropertyName];
            validationElements.Count.ShouldEqual(2);

            var validationResults = validationElements
                .SelectMany(element => element.Validate(validationContext))
                .ToArray();
            validationResults.Length.ShouldEqual(1);
            var validationResult = validationResults.Single();
            validationResult.MemberNames.SequenceEqual(result.MemberNames).ShouldBeTrue();
            validationResult.ErrorMessage.ShouldEqual(result.ErrorMessage);
        }

        #endregion
    }
}