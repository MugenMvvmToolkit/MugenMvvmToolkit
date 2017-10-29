#region Copyright

// ****************************************************************************
// <copyright file="DelegateContinuation.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    internal sealed class DelegateContinuation<TIn, TOut, TTarget> : IActionContinuation, IActionContinuation<TIn>,
        IFunctionContinuation<TOut>, IFunctionContinuation<TIn, TOut>
    {
        #region Fields

        private readonly Delegate _delegate;
        private readonly Action<IOperationResult> _action;
        private readonly Action<TTarget, IOperationResult> _actionWithTarget;

        private readonly Func<IOperationResult, TOut> _func;
        private readonly Func<TTarget, IOperationResult, TOut> _funcWithTarget;

        private readonly Action<IOperationResult<TIn>> _genericAction;
        private readonly Func<IOperationResult<TIn>, TOut> _genericFunc;

        private readonly Func<TTarget, IOperationResult<TIn>, TOut> _genericFuncWithTarget;
        private readonly Action<TTarget, IOperationResult<TIn>> _genericActionWithTarget;

        private bool _hasCallback;
        private ISerializableCallback _serializableCallback;

        #endregion

        #region Constructors

        public DelegateContinuation(Action<IOperationResult> action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
            _delegate = action;
        }

        public DelegateContinuation(Action<TTarget, IOperationResult> actionWithTarget)
        {
            Should.NotBeNull(actionWithTarget, nameof(actionWithTarget));
            _actionWithTarget = actionWithTarget;
            _delegate = actionWithTarget;
        }


        public DelegateContinuation(Action<IOperationResult<TIn>> genericAction)
        {
            Should.NotBeNull(genericAction, nameof(genericAction));
            _genericAction = genericAction;
            _delegate = genericAction;
        }

        public DelegateContinuation(Action<TTarget, IOperationResult<TIn>> genericActionWithTarget)
        {
            Should.NotBeNull(genericActionWithTarget, nameof(genericActionWithTarget));
            _genericActionWithTarget = genericActionWithTarget;
            _delegate = genericActionWithTarget;
        }


        public DelegateContinuation(Func<IOperationResult, TOut> func)
        {
            Should.NotBeNull(func, nameof(func));
            _func = func;
            _delegate = func;
        }

        public DelegateContinuation(Func<TTarget, IOperationResult, TOut> funcWithTarget)
        {
            Should.NotBeNull(funcWithTarget, nameof(funcWithTarget));
            _funcWithTarget = funcWithTarget;
            _delegate = funcWithTarget;
        }

        public DelegateContinuation(Func<IOperationResult<TIn>, TOut> genericFunc)
        {
            Should.NotBeNull(genericFunc, nameof(genericFunc));
            _genericFunc = genericFunc;
            _delegate = genericFunc;
        }

        public DelegateContinuation([NotNull] Func<TTarget, IOperationResult<TIn>, TOut> genericFuncWithTarget)
        {
            Should.NotBeNull(genericFuncWithTarget, nameof(genericFuncWithTarget));
            _genericFuncWithTarget = genericFuncWithTarget;
            _delegate = genericFuncWithTarget;
        }

        #endregion

        #region Implementation of IContinuation

        public ISerializableCallback ToSerializableCallback()
        {
            if (_hasCallback)
                return _serializableCallback;

            _hasCallback = true;
            _serializableCallback = ToolkitServiceProvider.OperationCallbackFactory.CreateSerializableCallback(_delegate);
            return _serializableCallback;
        }

        void IActionContinuation.Invoke(IOperationResult result)
        {
            if (_action == null)
                _actionWithTarget?.Invoke((TTarget)result.Source, result);
            else
                _action.Invoke(result);
        }

        void IActionContinuation<TIn>.Invoke(IOperationResult<TIn> result)
        {
            if (_genericAction == null)
                _genericActionWithTarget?.Invoke((TTarget)result.Source, result);
            else
                _genericAction(result);
        }

        TOut IFunctionContinuation<TIn, TOut>.Invoke(IOperationResult<TIn> result)
        {
            if (_genericFunc != null)
                return _genericFunc(result);
            if (_genericFuncWithTarget != null)
                return _genericFuncWithTarget((TTarget)result.Source, result);
            return default(TOut);
        }

        TOut IFunctionContinuation<TOut>.Invoke(IOperationResult result)
        {
            if (_func != null)
                return _func(result);
            if (_funcWithTarget != null)
                return _funcWithTarget((TTarget)result.Source, result);
            return default(TOut);
        }

        #endregion
    }
}
