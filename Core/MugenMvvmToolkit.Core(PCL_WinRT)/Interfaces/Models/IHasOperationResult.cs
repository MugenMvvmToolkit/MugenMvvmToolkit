#region Copyright
// ****************************************************************************
// <copyright file="IHasOperationResult.cs">
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
using System.ComponentModel;

namespace MugenMvvmToolkit.Interfaces.Models
{
    /// <summary>
    ///     Represents the model that has operation result.
    /// </summary>
    public interface IHasOperationResult : INotifyPropertyChanged
    {
        /// <summary>
        ///     Gets or sets the operation result value.
        /// </summary>
        bool? OperationResult { get; set; }
    }
}