#region Copyright

// ****************************************************************************
// <copyright file="IMvvmApplication.cs">
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
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    public interface IMvvmApplication
    {
        bool IsInitialized { get; }

        [NotNull]
        PlatformInfo Platform { get; }

        LoadMode Mode { get; }

        IIocContainer IocContainer { get; }

        IDataContext Context { get; }

        IViewModelSettings ViewModelSettings { get; }

        void Initialize(PlatformInfo platform, IIocContainer iocContainer, IList<Assembly> assemblies, IDataContext context);

        [NotNull]
        Type GetStartViewModelType();
    }
}
