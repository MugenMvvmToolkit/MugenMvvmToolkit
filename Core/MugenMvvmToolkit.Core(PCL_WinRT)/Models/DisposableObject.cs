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
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the base class that notifies when it is disposed.
    /// </summary>
    public abstract class DisposableObject : IDisposableObject
    {
        #region Fields

        private const int DisposedState = 1;
        private int _disposed;
        private bool _isDisposed;

        #endregion

        #region Destructor

        ~DisposableObject()
        {
            _isDisposed = true;
            OnDispose(false);
            MvvmUtils.TraceFinalizedItem(this);
        }

        #endregion

        #region Implementation of IDisposable

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, DisposedState) == DisposedState)
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
                _isDisposed = true;
            }
        }

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
            var handler = Disposed;
            if (handler != null) 
                handler(this, EventArgs.Empty);
        }

        #endregion

        #region Implementation of IDisposableObject

        /// <summary>
        ///     Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get { return _isDisposed; }
        }

        /// <summary>
        ///     Occurs when the object is disposed by a call to the Dispose method.
        /// </summary>
        public event EventHandler<IDisposableObject, EventArgs> Disposed;

        #endregion
    }
}