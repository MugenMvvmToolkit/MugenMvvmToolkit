#region Copyright

// ****************************************************************************
// <copyright file="INavigationProviderEx.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Navigation
{
    /// <summary>
    ///     Represent the interface for navigation provider.
    /// </summary>
    public interface INavigationProviderEx : INavigationProvider
    {
        /// <summary>
        ///     Gets the <see cref="INavigationService" />.
        /// </summary>
        [NotNull]
        INavigationService NavigationService { get; }
    }
}