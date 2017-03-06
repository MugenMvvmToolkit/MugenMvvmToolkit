#region Copyright

// ****************************************************************************
// <copyright file="INavigationContext.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    public interface INavigationContext : IDataContext
    {
        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }

        [CanBeNull]
        IViewModel ViewModelFrom { get; }

        [CanBeNull]
        IViewModel ViewModelTo { get; }

        [CanBeNull]
        object NavigationProvider { get; }
    }
}
