#region Copyright

// ****************************************************************************
// <copyright file="IModuleContext.cs">
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

using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Models
{
    public interface IModuleContext
    {
        [CanBeNull]
        IIocContainer IocContainer { get; }

        [NotNull]
        IDataContext Context { get; }

        [NotNull]
        IList<Assembly> Assemblies { get; }

        LoadMode Mode { get; }

        [NotNull]
        PlatformInfo Platform { get; }
    }
}
