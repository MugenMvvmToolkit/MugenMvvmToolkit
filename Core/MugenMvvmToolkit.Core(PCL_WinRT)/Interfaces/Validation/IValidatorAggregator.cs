#region Copyright

// ****************************************************************************
// <copyright file="IValidatorAggregator.cs">
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
using System.ComponentModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface IValidatorAggregator : IDisposableObject, INotifyDataErrorInfo
#if NONOTIFYDATAERROR
      ,IDataErrorInfo
#endif
    {
        [NotNull]
        Func<object, IValidatorContext> CreateContext { get; set; }

        [NotNull]
        IDictionary<string, ICollection<string>> PropertyMappings { get; }

        [NotNull]
        ICollection<string> IgnoreProperties { get; }

        ManualValidator Validator { get; }

        bool IsValid { get; }

        [NotNull]
        IList<IValidator> GetValidators();

        void AddValidator([NotNull] IValidator validator);

        bool RemoveValidator([NotNull] IValidator validator);

        void AddInstance([NotNull] object instanceToValidate);

        bool RemoveInstance([NotNull] object instanceToValidate);

        Task ValidateInstanceAsync([NotNull] object instanceToValidate);

        Task ValidateAsync([NotNull] string propertyName);

        Task ValidateAsync();

#if !NONOTIFYDATAERROR
        IList<object> this[string propertyName] { get; }
#endif

        new IList<object> GetErrors(string propertyName);

        [NotNull]
        IDictionary<string, IList<object>> GetErrors();

        void ClearErrors([NotNull] string propertyName);

        void ClearErrors();
    }
}
