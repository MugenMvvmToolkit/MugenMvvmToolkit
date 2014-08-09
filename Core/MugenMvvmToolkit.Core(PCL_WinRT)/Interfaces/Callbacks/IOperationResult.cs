#region Copyright
// ****************************************************************************
// <copyright file="IOperationResult.cs">
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
using System;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Represents the result of operation.
    /// </summary>
    public interface IOperationResult
    {
        /// <summary>
        ///     Gets the type of operation.
        /// </summary>
        [NotNull]
        OperationType Operation { get; }

        /// <summary>
        ///     Gets the source of the operation.
        /// </summary>
        [NotNull]
        object Source { get; }

        /// <summary>
        ///     Gets the exception that caused the operartion to end prematurely.
        ///     If the operation completed successfully or has not yet thrown any exceptions, this will return null.
        /// </summary>
        [CanBeNull]
        Exception Exception { get; }

        /// <summary>
        ///     Gets whether the operation has completed execution due to being canceled.
        /// </summary>
        bool IsCanceled { get; }

        /// <summary>
        ///     Gets whether the operation completed due to an unhandled exception.
        /// </summary>
        bool IsFaulted { get; }

        /// <summary>
        ///     Gets the result value of this operation.
        /// </summary>
        [CanBeNull]
        object Result { get; }

        /// <summary>
        ///     Gets the context of the operation.
        /// </summary>
        [NotNull]
        IDataContext OperationContext { get; }
    }

    /// <summary>
    ///     Represents the result of operation.
    /// </summary>
    public interface IOperationResult<out TResult> : IOperationResult
    {
        /// <summary>
        ///     Gets the result value of this operation.
        /// </summary>
        [CanBeNull]
        new TResult Result { get; }
    }
}