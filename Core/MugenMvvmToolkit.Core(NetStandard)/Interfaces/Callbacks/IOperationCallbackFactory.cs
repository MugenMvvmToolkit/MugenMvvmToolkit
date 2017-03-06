#region Copyright

// ****************************************************************************
// <copyright file="IOperationCallbackFactory.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
    public interface IOperationCallbackFactory
    {
        [NotNull]
        IAsyncOperationAwaiter CreateAwaiter([NotNull] IAsyncOperation operation, [CanBeNull] IDataContext context);

        [NotNull]
        IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>([NotNull] IAsyncOperation<TResult> operation, [CanBeNull] IDataContext context);

        [CanBeNull]
        ISerializableCallback CreateSerializableCallback([NotNull] Delegate @delegate);
    }
}
