#region Copyright

// ****************************************************************************
// <copyright file="IValidatorContext.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Interfaces.Validation
{
    public interface IValidatorContext : IServiceProvider
    {
        [NotNull]
        object Instance { get; }

        [NotNull]
        IDataContext ValidationMetadata { get; }

        [NotNull]
        IDictionary<string, ICollection<string>> PropertyMappings { get; }

        [NotNull]
        ICollection<string> IgnoreProperties { get; }

        [CanBeNull]
        IServiceProvider ServiceProvider { get; }
    }
}
