#region Copyright

// ****************************************************************************
// <copyright file="IValidatorProvider.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface IValidatorProvider
    {
        [Pure]
        bool IsRegistered([NotNull] Type validatorType);

        void Register([NotNull] Type validatorType);

        bool Unregister([NotNull] Type validatorType);

        [NotNull]
        IList<IValidator> GetValidators([NotNull] IValidatorContext context);

        [NotNull]
        IValidatorAggregator GetValidatorAggregator();
    }
}
