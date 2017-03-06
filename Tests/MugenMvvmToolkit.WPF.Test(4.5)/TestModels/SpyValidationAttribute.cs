#region Copyright

// ****************************************************************************
// <copyright file="SpyValidationAttribute.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
