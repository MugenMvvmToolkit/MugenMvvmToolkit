#region Copyright

// ****************************************************************************
// <copyright file="AsyncValidationMessage.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Threading.Tasks;

namespace MugenMvvmToolkit.Models.Messages
{
    public class AsyncValidationMessage
    {
        #region Fields

        private static readonly TaskCompletionSource<object> EmptyTcs;
        private readonly string _propertyName;
        private TaskCompletionSource<object> _tcs;

        #endregion

        #region Constructors

        static AsyncValidationMessage()
        {
            EmptyTcs = new TaskCompletionSource<object>();
            EmptyTcs.SetResult(null);
        }

        public AsyncValidationMessage(string propertyName)
        {
            _propertyName = propertyName;
        }

        #endregion

        #region Properties

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public Task Task
        {
            get
            {
                if (_tcs == null)
                    Interlocked.CompareExchange(ref _tcs, new TaskCompletionSource<object>(), null);
                return _tcs.Task;
            }
        }

        #endregion

        #region Methods

        protected internal void SetCompleted(Exception exception, bool canceled)
        {
            if (_tcs == null)
                Interlocked.CompareExchange(ref _tcs, exception == null && !canceled ? EmptyTcs : new TaskCompletionSource<object>(), null);
            if (exception == null && !canceled)
                _tcs.TrySetResult(null);
            else
            {
                if (canceled)
                    _tcs.SetCanceled();
                else
                    _tcs.TrySetException(exception);
            }
        }

        #endregion
    }
}
