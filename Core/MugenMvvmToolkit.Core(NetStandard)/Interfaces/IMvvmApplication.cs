#region Copyright

// ****************************************************************************
// <copyright file="IMvvmApplication.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IMvvmApplication
    {
        ApplicationState ApplicationState { get; }

        bool IsInitialized { get; }

        [NotNull]
        PlatformInfo PlatformInfo { get; }

        [NotNull]
        IDataContext Context { get; }

        IIocContainer IocContainer { get; }

        void Initialize(PlatformInfo platformInfo, IIocContainer iocContainer, IList<Assembly> assemblies, IDataContext context);

        void SetApplicationState(ApplicationState value, [CanBeNull] IDataContext context);

        void Start();
    }
}
