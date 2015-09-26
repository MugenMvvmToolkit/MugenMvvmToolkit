#region Copyright

// ****************************************************************************
// <copyright file="IValidator.cs">
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

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface IValidator : IDisposable, INotifyDataErrorInfo
#if NONOTIFYDATAERROR
, IDataErrorInfo
#endif
    {
        bool ValidateOnPropertyChanged { get; set; }

        bool IsDisposed { get; }

        bool IsInitialized { get; }

        bool IsValid { get; }

        [CanBeNull]
        IValidatorContext Context { get; }

        bool Initialize([NotNull] IValidatorContext context);

        [NotNull]
        new IList<object> GetErrors([CanBeNull]string propertyName);

        [NotNull, Pure]
        IDictionary<string, IList<object>> GetErrors();

        [NotNull]
        Task ValidateAsync([CanBeNull] string propertyName);

        [NotNull]
        Task ValidateAsync();

        void CancelValidation();

        void ClearErrors([CanBeNull]string propertyName);

        void ClearErrors();
    }
}
