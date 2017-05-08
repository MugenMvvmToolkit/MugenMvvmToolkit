﻿#region Copyright

// ****************************************************************************
// <copyright file="IOpenedViewModelInfo.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface IOpenedViewModelInfo
    {
        [NotNull]
        IViewModel ViewModel { get; }

        object NavigationProvider { get; }

        [NotNull]
        NavigationType NavigationType { get; }
    }
}