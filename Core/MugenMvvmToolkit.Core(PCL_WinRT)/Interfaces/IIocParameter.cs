#region Copyright

// ****************************************************************************
// <copyright file="IIocParameter.cs">
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

using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the base interface for all ioc parameters.
    /// </summary>
    public interface IIocParameter
    {
        /// <summary>
        ///     Gets the parameter name.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the parameter value.
        /// </summary>
        object Value { get; }

        /// <summary>
        ///     Gets the parameter type.
        /// </summary>
        IocParameterType ParameterType { get; }
    }
}