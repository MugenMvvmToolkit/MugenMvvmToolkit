#region Copyright

// ****************************************************************************
// <copyright file="DisposableObject.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the base class that notifies when it is disposed.
    /// </summary>
    public abstract class DisposableObject : IDisposableObject
    {
        #region Fields

        private const int DisposingState = 1;
        private const int DisposedState = 2;
        private int _disposed;

        #endregion

        #region Methods

        /// <summary>
        ///     Makes sure that the object is not disposed.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            this.NotBeDisposed();
        }

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected virtual void OnDispose(bool disposing)
        {
        }

        private void RaiseDisposed()
        {
            EventHandler<IDisposableObject, EventArgs> handler = Disposed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Implementation of IDisposableObject

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposed, DisposingState, 0) != 0)
                return;
            try
            {
                GC.SuppressFinalize(this);
                OnDispose(true);
                RaiseDisposed();
                Disposed = null;
            }
            finally
            {
                _disposed = DisposedState;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _disposed == DisposedState; }
        }

        /// <summary>
        ///     Occurs when the object is disposed by a call to the Dispose method.
        /// </summary>
        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        #endregion

        #region Destructor

        ~DisposableObject()
        {
            OnDispose(false);
            Tracer.Finalized(this);
        }

        #endregion
    }
}