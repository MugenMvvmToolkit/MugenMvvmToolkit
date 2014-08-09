using System;
using System.Threading;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ThreadManagerMock : IThreadManager
    {
        #region Properties

        public Action InvokeOnUiThreadAsync { get; set; }

        public Action InvokeOnUiThread { get; set; }

        public Action InvokeAsync { get; set; }

        public bool ImmediateInvokeOnUiThread { get; set; }

        public bool ImmediateInvokeOnUiThreadAsync { get; set; }

        public bool ImmediateInvokeAsync { get; set; }

        #endregion

        #region Implementation of IThreadManager

        public bool IsUiThread { get; set; }

        void IThreadManager.InvokeOnUiThreadAsync(Action action, OperationPriority priority,
            CancellationToken cancellationToken)
        {
            InvokeOnUiThreadAsync = action;
            if (ImmediateInvokeOnUiThreadAsync)
                action();
        }

        void IThreadManager.InvokeOnUiThread(Action action, OperationPriority priority,
            CancellationToken cancellationToken)
        {
            InvokeOnUiThread = action;
            if (ImmediateInvokeOnUiThread)
                action();
        }

        void IThreadManager.InvokeAsync(Action action, OperationPriority priority, CancellationToken cancellationToken)
        {
            InvokeAsync = action;
            if (ImmediateInvokeAsync)
                InvokeAsync();
        }

        #endregion
    }
}