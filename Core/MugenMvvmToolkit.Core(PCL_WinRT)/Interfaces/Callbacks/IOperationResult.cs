#region Copyright

// ****************************************************************************
// <copyright file="IOperationResult.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public interface IOperationResult
    {
        [NotNull]
        OperationType Operation { get; }

        [NotNull]
        object Source { get; }

        [CanBeNull]
        Exception Exception { get; }

        bool IsCanceled { get; }

        bool IsFaulted { get; }

        [CanBeNull]
        object Result { get; }

        [NotNull]
        IDataContext OperationContext { get; }
    }

    public interface IOperationResult<out TResult> : IOperationResult
    {
        [CanBeNull]
        new TResult Result { get; }
    }
}
