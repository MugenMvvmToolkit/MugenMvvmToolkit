#region Copyright
// ****************************************************************************
// <copyright file="Enums.cs">
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
namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the actions that a binding can perform.
    /// </summary>
    public enum BindingAction
    {
        /// <summary>
        ///     The binding is currently update the source.
        /// </summary>
        UpdateSource = 1,

        /// <summary>
        ///     The binding is currently update the target.
        /// </summary>
        UpdateTarget = 2,
    }
}