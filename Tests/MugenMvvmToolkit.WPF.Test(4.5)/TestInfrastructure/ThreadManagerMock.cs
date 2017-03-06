#region Copyright

// ****************************************************************************
// <copyright file="ThreadManagerMock.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ThreadManagerMock : IThreadManager
    {
        #region Fields

        private Action _invokeAsync;
        private Action _invokeOnUiThread;
        private Action _invokeOnUiThreadAsync;

        #endregion

        #region Properties

        public bool ImmediateInvokeOnUiThread { get; set; }

        public bool ImmediateInvokeOnUiThreadAsync { get; set; }

        public bool ImmediateInvokeAsync { get; set; }

        #endregion

        #region Method

        public void InvokeAsync()
        {
            var oldValue = IsUiThread;
            try
            {
                IsUiThread = false;
                var action = _invokeAsync;
                _invokeAsync = null;
                action?.Invoke();
            }
            finally
            {
                IsUiThread = oldValue;
            }
        }

        public void InvokeOnUiThread()
        {
            var oldValue = IsUiThread;
            try
            {
                IsUiThread = true;
                var action = _invokeOnUiThread;
                _invokeOnUiThread = null;
                action?.Invoke();
            }
            finally
            {
                _invokeOnUiThread = null;
                IsUiThread = oldValue;
            }
        }

        public void InvokeOnUiThreadAsync()
        {
            var oldValue = IsUiThread;
            try
            {
                IsUiThread = true;
                var action = _invokeOnUiThreadAsync;
                _invokeOnUiThreadAsync = null;
                action?.Invoke();
            }
            finally
            {
                IsUiThread = oldValue;
            }
        }

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread { get; set; }

        void IThreadManager.InvokeOnUiThreadAsync(Action action, OperationPriority priority,
            CancellationToken cancellationToken)
        {
            _invokeOnUiThreadAsync += action;
            if (ImmediateInvokeOnUiThreadAsync)
                InvokeOnUiThreadAsync();
        }

        void IThreadManager.InvokeOnUiThread(Action action, OperationPriority priority,
            CancellationToken cancellationToken)
        {
            _invokeOnUiThread += action;
            if (ImmediateInvokeOnUiThread)
                InvokeOnUiThread();
        }

        void IThreadManager.InvokeAsync(Action action, OperationPriority priority, CancellationToken cancellationToken)
        {
            _invokeAsync += action;
            if (ImmediateInvokeAsync)
                InvokeAsync();
        }

        #endregion
    }
}