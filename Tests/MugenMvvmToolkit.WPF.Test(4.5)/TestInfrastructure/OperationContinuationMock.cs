#region Copyright

// ****************************************************************************
// <copyright file="OperationContinuationMock.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public abstract class ContinuationMockBase : IContinuation
    {
        #region Properties

        public Func<ISerializableCallback> ToSerializableCallback { get; set; }

        #endregion

        #region Implementation of IContinuation

        ISerializableCallback IContinuation.ToSerializableCallback()
        {
            if (ToSerializableCallback == null)
                return null;
            return ToSerializableCallback();
        }

        #endregion
    }

    public class ActionContinuationMock : ContinuationMockBase, IActionContinuation
    {
        #region Properties

        public Action<IOperationResult> Invoke { get; set; }

        #endregion

        #region Implementation of IContinuation

        void IActionContinuation.Invoke(IOperationResult result)
        {
            if (Invoke != null)
                Invoke(result);
        }

        #endregion
    }

    public class ActionContinuationMock<TResult> : ContinuationMockBase, IActionContinuation<TResult>
    {
        #region Properties

        public Action<IOperationResult<TResult>> Invoke { get; set; }

        #endregion

        #region Implementation of IContinuation

        void IActionContinuation<TResult>.Invoke(IOperationResult<TResult> result)
        {
            if (Invoke != null)
                Invoke(result);
        }

        #endregion
    }

    public class FunctionContinuationMock<TResult> : ContinuationMockBase, IFunctionContinuation<TResult>
    {
        #region Properties

        public Func<IOperationResult, TResult> Invoke { get; set; }

        #endregion

        #region Implementation of IContinuation

        TResult IFunctionContinuation<TResult>.Invoke(IOperationResult result)
        {
            if (Invoke == null)
                return default(TResult);
            return Invoke(result);
        }

        #endregion
    }

    public class FunctionContinuationMock<TIn, TOut> : ContinuationMockBase, IFunctionContinuation<TIn, TOut>
    {
        #region Properties

        public Func<IOperationResult<TIn>, TOut> Invoke { get; set; }

        #endregion

        #region Implementation of IContinuation

        TOut IFunctionContinuation<TIn, TOut>.Invoke(IOperationResult<TIn> result)
        {
            if (Invoke == null)
                return default(TOut);
            return Invoke(result);
        }

        #endregion
    }
}
