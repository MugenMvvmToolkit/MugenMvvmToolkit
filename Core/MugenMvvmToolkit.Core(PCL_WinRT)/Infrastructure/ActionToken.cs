#region Copyright

// ****************************************************************************
// <copyright file="ActionToken.cs">
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

using System;
using System.Threading;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Infrastructure
{
    public sealed class ActionToken : IDisposable
    {
        #region Fields

        private const int DisposedState = 1;

        private readonly object _state;
        private object _action;
        private int _disposed;

        #endregion

        #region Constructors

        public ActionToken([NotNull] Action action)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
        }

        public ActionToken([NotNull] Action<object> action, object state)
        {
            Should.NotBeNull(action, nameof(action));
            _action = action;
            _state = state;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, DisposedState) == DisposedState)
                return;
            var action = _action as Action<object>;
            if (action == null)
                ((Action)_action).Invoke();
            else
                action.Invoke(_state);
            _action = null;
        }

        #endregion
    }
}
