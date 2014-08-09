#region Copyright
// ****************************************************************************
// <copyright file="IDesignViewModel.cs">
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
namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the design view-model.
    /// </summary>
    public interface IDesignViewModel : IViewModel
    {
        /// <summary>
        ///     Initializes the current view model in design mode.
        /// </summary>
        void InitializeViewModel();
    }
}