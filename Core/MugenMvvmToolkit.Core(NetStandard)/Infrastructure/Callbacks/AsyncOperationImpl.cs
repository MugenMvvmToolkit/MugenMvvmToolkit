#region Copyright

// ****************************************************************************
// <copyright file="AsyncOperationImpl.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    internal sealed class AsyncOperationImpl<T, TIn> : AsyncOperation<T>
    {
        #region Fields

        private readonly IActionContinuation _continuationAction;
        private readonly IActionContinuation<T> _continuationActionGeneric;
        private readonly IFunctionContinuation<T> _continuationFunction;
        private readonly IFunctionContinuation<TIn, T> _continuationFunctionGeneric;

        #endregion

        #region Constructors

        public AsyncOperationImpl([NotNull] IActionContinuation continuationAction, IDataContext context)
            : base(context)
        {
            Should.NotBeNull(continuationAction, nameof(continuationAction));
            _continuationAction = continuationAction;
        }

        public AsyncOperationImpl([NotNull] IActionContinuation<T> continuationAction, IDataContext context)
            : base(context)
        {
            Should.NotBeNull(continuationAction, nameof(continuationAction));
            _continuationActionGeneric = continuationAction;
        }

        public AsyncOperationImpl([NotNull] IFunctionContinuation<T> continuationFunction, IDataContext context)
            : base(context)
        {
            Should.NotBeNull(continuationFunction, nameof(continuationFunction));
            _continuationFunction = continuationFunction;
        }

        public AsyncOperationImpl([NotNull] IFunctionContinuation<TIn, T> continuationFunction, IDataContext context)
            : base(context)
        {
            Should.NotBeNull(continuationFunction, nameof(continuationFunction));
            _continuationFunctionGeneric = continuationFunction;
        }

        #endregion

        #region Overrides of AsyncOperation<T>

        internal override IOperationResult<T> InvokeInternal(IOperationResult result)
        {
            if (_continuationAction != null)
            {
                _continuationAction.Invoke(result);
                return OperationResult.Convert<T>(result);
            }
            if (_continuationActionGeneric != null)
            {
                IOperationResult<T> genericResult = OperationResult.Convert<T>(result);
                _continuationActionGeneric.Invoke(genericResult);
                return genericResult;
            }
            if (_continuationFunction != null)
                return OperationResult.CreateResult(result.Operation, result.Source,
                    _continuationFunction.Invoke(result), result.OperationContext);

            return OperationResult.CreateResult(result.Operation, result.Source,
                _continuationFunctionGeneric.Invoke(OperationResult.Convert<TIn>(result)), result.OperationContext);
        }

        internal override ISerializableCallback ToSerializableCallbackInternal()
        {
            if (IsCompleted)
                return null;
            ISerializableCallback callback;
            bool isFunc = false;
            string inputTypeName = null;
            if (_continuationAction != null)
                callback = _continuationAction.ToSerializableCallback();
            else if (_continuationActionGeneric != null)
            {
                callback = _continuationActionGeneric.ToSerializableCallback();
                inputTypeName = typeof(T).AssemblyQualifiedName;
            }
            else if (_continuationFunction != null)
            {
                callback = _continuationFunction.ToSerializableCallback();
                inputTypeName = typeof(T).AssemblyQualifiedName;
                isFunc = true;
            }
            else
            {
                callback = _continuationFunctionGeneric.ToSerializableCallback();
                inputTypeName = typeof(TIn).AssemblyQualifiedName;
                isFunc = true;
            }
            if (callback == null)
                return null;
            return new AsyncOperationSerializableCallback(callback, inputTypeName, typeof(T).AssemblyQualifiedName,
                isFunc, GetContinuationsCallbacks());
        }

        #endregion
    }
}
