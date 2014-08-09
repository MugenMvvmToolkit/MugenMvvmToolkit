#region Copyright
// ****************************************************************************
// <copyright file="IOperationCallbackFactory.cs">
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

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    /// <summary>
    ///     Rerpresets the factory that allows to create callback operations.
    /// </summary>
    public interface IOperationCallbackFactory
    {
        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter" />.
        /// </summary>
        [NotNull]
        IAsyncOperationAwaiter CreateAwaiter([NotNull] IAsyncOperation operation, [CanBeNull] IDataContext context);

        /// <summary>
        ///     Creates an instance of <see cref="IAsyncOperationAwaiter{TResult}" />.
        /// </summary>
        [NotNull]
        IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>([NotNull] IAsyncOperation<TResult> operation, [CanBeNull] IDataContext context);

        /// <summary>
        ///     Tries to convert a delegate to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        [CanBeNull]
        ISerializableCallback CreateSerializableCallback([NotNull] Delegate @delegate);
    }
}