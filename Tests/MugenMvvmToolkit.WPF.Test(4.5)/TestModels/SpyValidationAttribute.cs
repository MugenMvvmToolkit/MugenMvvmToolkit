using System;
using System.ComponentModel.DataAnnotations;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class SpyValidationAttribute : ValidationAttribute
    {
        #region Properties

        public static Func<object, ValidationContext, ValidationResult> IsValidDelegate { get; set; }

        #endregion

        #region Overrides of ValidationAttribute

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return IsValidDelegate(value, validationContext);
        }

        #endregion
    }
}
