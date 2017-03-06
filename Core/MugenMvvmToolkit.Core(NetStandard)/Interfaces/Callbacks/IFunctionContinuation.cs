#region Copyright

// ****************************************************************************
// <copyright file="IFunctionContinuation.cs">
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

namespace MugenMvvmToolkit.Interfaces.Callbacks
{
    public interface IFunctionContinuation<out TResult> : IContinuation
    {
        TResult Invoke(IOperationResult result);
    }

    public interface IFunctionContinuation<in TResult, out TNewResult> : IContinuation
    {
        TNewResult Invoke(IOperationResult<TResult> result);
    }
}
