#region Copyright
// ****************************************************************************
// <copyright file="IHasState.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the model that has state.
    /// </summary>
    public interface IHasState
    {
        /// <summary>
        ///     Loads state.
        /// </summary>
        void LoadState([NotNull] IDataContext state);

        /// <summary>
        ///     Saves state.
        /// </summary>
        void SaveState([NotNull] IDataContext state);
    }
}