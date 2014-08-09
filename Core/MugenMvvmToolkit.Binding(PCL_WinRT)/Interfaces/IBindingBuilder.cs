#region Copyright
// ****************************************************************************
// <copyright file="IBindingBuilder.cs">
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
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Interfaces
{
    /// <summary>
    ///     Represents the data binding builder.
    /// </summary>
    public interface IBindingBuilder : IDataContext
    {
        /// <summary>
        ///     Builds an instance of <see cref="IDataBinding" />.
        /// </summary>
        /// <returns>
        ///     The builded <see cref="IDataBinding" />.
        /// </returns>
        [NotNull]
        IDataBinding Build();
    }
}