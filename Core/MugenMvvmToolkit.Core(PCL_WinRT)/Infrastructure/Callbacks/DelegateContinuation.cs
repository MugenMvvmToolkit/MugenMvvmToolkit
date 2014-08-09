#region Copyright
// ****************************************************************************
// <copyright file="DelegateContinuation.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    internal sealed class DelegateContinuation<TIn, TOut, TTarget> : IActionContinuation, IActionContinuation<TIn>,
        IFunctionContinuation<TOut>, IFunctionContinuation<TIn, TOut>
    {
        #region Fields

        private readonly Delegate _delegate;
        private readonly Action<IOperationResult> _action;
        private readonly Func<IOperationResult, TOut> _func;
        private readonly Action<IOperationResult<TIn>> _genericAction;
        private readonly Func<IOperationResult<TIn>, TOut> _genericFunc;
        private readonly Func<TTarget, IOperationResult<TIn>, TOut> _funcWithTarget;
        private readonly Action<TTarget, IOperationResult<TIn>> _actionWithTarget;

        private bool _hasCallback;
        private ISerializableCallback _serializableCallback;

        #endregion

        #region Constructors

        public DelegateContinuation(Action<IOperationResult> action)
        {
            Should.NotBeNull(action, "action");
            _action = action;
            _delegate = action;
        }

        public DelegateContinuation(Action<IOperationResult<TIn>> genericAction)
        {
            Should.NotBeNull(genericAction, "genericAction");
            _genericAction = genericAction;
            _delegate = genericAction;
        }

        public DelegateContinuation(Func<IOperationResult, TOut> func)
        {
            Should.NotBeNull(func, "func");
            _func = func;
            _delegate = func;
        }

        public DelegateContinuation(Func<IOperationResult<TIn>, TOut> genericFunc)
        {
            Should.NotBeNull(genericFunc, "genericFunc");
            _genericFunc = genericFunc;
            _delegate = genericFunc;
        }

        public DelegateContinuation(Action<TTarget, IOperationResult<TIn>> actionWithTarget)
        {
            Should.NotBeNull(actionWithTarget, "actionWithTarget");
            _actionWithTarget = actionWithTarget;
            _delegate = actionWithTarget;
        }

        public DelegateContinuation([NotNull] Func<TTarget, IOperationResult<TIn>, TOut> funcWithTarget)
        {
            Should.NotBeNull(funcWithTarget, "funcWithTarget");
            _funcWithTarget = funcWithTarget;
            _delegate = funcWithTarget;
        }

        #endregion

        #region Implementation of IContinuation

        /// <summary>
        ///     Tries to convert current operation to an instance of <see cref="ISerializableCallback" />.
        /// </summary>
        public ISerializableCallback ToSerializableCallback()
        {
            if (_hasCallback)
                return _serializableCallback;

            _hasCallback = true;
            _serializableCallback = ServiceProvider.OperationCallbackFactory.CreateSerializableCallback(_delegate);
            return _serializableCallback;
        }

        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
        void IActionContinuation.Invoke(IOperationResult result)
        {
            if (_action != null)
                _action(result);
        }

        /// <summary>
        ///     Invokes the action using the specified operation result.
        /// </summary>
        void IActionContinuation<TIn>.Invoke(IOperationResult<TIn> result)
        {
            if (_genericAction != null)
                _genericAction(result);
            else if (_actionWithTarget != null)
                _actionWithTarget((TTarget)result.Source, result);
        }

        /// <summary>
        ///     Invokes the function using the specified operation result.
        /// </summary>
        TOut IFunctionContinuation<TIn, TOut>.Invoke(IOperationResult<TIn> result)
        {
            if (_genericFunc != null)
                return _genericFunc(result);
            if (_funcWithTarget != null)
                return _funcWithTarget((TTarget)result.Source, result);
            return default(TOut);
        }

        /// <summary>
        ///     Invokes the function using the specified operation result.
        /// </summary>
        TOut IFunctionContinuation<TOut>.Invoke(IOperationResult result)
        {
            if (_func != null)
                return _func(result);
            return default(TOut);
        }

        #endregion
    }
}