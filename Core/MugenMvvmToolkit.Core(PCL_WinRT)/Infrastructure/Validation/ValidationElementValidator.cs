#region Copyright

// ****************************************************************************
// <copyright file="ValidationElementValidator.cs">
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models.Validation;

namespace MugenMvvmToolkit.Infrastructure.Validation
{
    /// <summary>
    ///     Represents a validator that uses a <see cref="IValidationElementProvider" /> to validate objects.
    /// </summary>
    public class ValidationElementValidator : ValidatorBase<object>
    {
        #region Fields

        private IValidationElementProvider _validationElementProvider;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="IValidationElementProvider"/>.
        /// </summary>
        public IValidationElementProvider ValidationElementProvider
        {
            get { return _validationElementProvider; }
        }

        #endregion

        #region Overrides of ValidatorBase

        /// <summary>
        ///     Checks to see whether the validator can validate objects of the specified IValidatorContext.
        /// </summary>
        internal override bool CanValidateContext(IValidatorContext validatorContext)
        {
            if (_validationElementProvider == null)
            {
                var iocContainer = validatorContext.ServiceProvider as IIocContainer ?? ServiceProvider.IocContainer;
                if (iocContainer == null || !iocContainer.TryGet(out _validationElementProvider))
                    return false;
            }
            return _validationElementProvider.GetValidationElements(validatorContext.Instance).Count > 0;
        }

        /// <summary>
        ///     Updates information about errors in the specified property.
        /// </summary>
        /// <param name="propertyName">The specified property name.</param>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync(string propertyName)
        {
            IList<IValidationElement> elements;
            if (!ValidationElementProvider
                .GetValidationElements(Instance)
                .TryGetValue(propertyName, out elements) || elements == null)
                return DoNothingResult;
            IValidationContext context = CreateContext();
            var result = new Dictionary<string, IEnumerable>();
            Validate(result, elements, context);
            return FromResult(result);
        }

        /// <summary>
        ///     Updates information about all errors.
        /// </summary>
        /// <returns>
        ///     The result of validation.
        /// </returns>
        protected override Task<IDictionary<string, IEnumerable>> ValidateInternalAsync()
        {
            var elements = ValidationElementProvider.GetValidationElements(Instance);
            if (elements.Count == 0)
                return DoNothingResult;
            var result = new Dictionary<string, IEnumerable>();
            IValidationContext context = CreateContext();
            foreach (var element in elements)
                Validate(result, element.Value, context);
            return FromResult(result);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Creates an instance of <see cref="IValidationContext" />.
        /// </summary>
        protected virtual IValidationContext CreateContext()
        {
            return new ValidationContext(Instance, Context.ServiceProvider, Context.ValidationMetadata.ToDictionary());
        }

        private static void Validate(Dictionary<string, IEnumerable> result, ICollection<IValidationElement> elements, IValidationContext context)
        {
            var validationResults = ValidateElements(elements, context);
            for (int index = 0; index < validationResults.Count; index++)
            {
                var validationResult = validationResults[index];
                if (!validationResult.MemberNames.Any())
                {
                    Tracer.Warn("The validation result for member '{0}' does not contain any MemberNames, ErrorMessage '{1}'.",
                        context.MemberName, validationResult.ErrorMessage);
                    continue;
                }
                foreach (var member in validationResult.MemberNames)
                {
                    if (member == null)
                        continue;
                    IEnumerable value;
                    if (!result.TryGetValue(member, out value) || !(value is List<object>))
                    {
                        var objects = new List<object>();
                        if (value != null)
                            objects.AddRange(value.OfType<object>());
                        result[member] = objects;
                        value = objects;
                    }
                    ((IList)value).Add(validationResult);
                }
            }
        }

        private static IList<IValidationResult> ValidateElements(ICollection<IValidationElement> elements, IValidationContext context)
        {
            if (elements.Count == 0)
                return Empty.Array<IValidationResult>();
            var results = new List<IValidationResult>();
            foreach (IValidationElement validationElement in elements)
            {
                IEnumerable<IValidationResult> validationResults = validationElement.Validate(context);
                if (validationResults == null)
                    continue;
                foreach (var result in validationResults)
                {
                    if (result != null && !result.IsValid)
                        results.Add(result);
                }
            }
            return results;
        }

        #endregion
    }
}