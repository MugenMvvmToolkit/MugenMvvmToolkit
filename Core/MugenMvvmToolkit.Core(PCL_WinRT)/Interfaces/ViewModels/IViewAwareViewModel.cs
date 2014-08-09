#region Copyright
// ****************************************************************************
// <copyright file="IViewAwareViewModel.cs">
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
using MugenMvvmToolkit.Interfaces.Views;

namespace MugenMvvmToolkit.Interfaces.ViewModels
{
    /// <summary>
    ///     Represents the interface that adds support for the view in view models.
    /// </summary>
    public interface IViewAwareViewModel<TView> : IViewModel where TView : IView
    {
        /// <summary>
        ///     Gets or sets the <see cref="IView" />.
        /// </summary>
        TView View { get; set; }
    }
}