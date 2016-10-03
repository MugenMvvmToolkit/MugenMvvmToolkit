#region Copyright

// ****************************************************************************
// <copyright file="IActionContinuation.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    public interface IActionContinuation : IContinuation
    {
        void Invoke([NotNull] IOperationResult result);
    }

    public interface IActionContinuation<in TResult> : IContinuation
    {
        void Invoke([NotNull] IOperationResult<TResult> result);
    }
}
