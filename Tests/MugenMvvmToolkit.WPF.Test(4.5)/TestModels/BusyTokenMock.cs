#region Copyright

// ****************************************************************************
// <copyright file="BusyTokenMock.cs">
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

using System.Collections.Generic;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class BusyTokenMock : IBusyToken
    {
        #region Fields

        private readonly List<IBusyTokenCallback> _handlers;

        #endregion

        #region Constructors

        public BusyTokenMock(object message = null)
        {
            _handlers = new List<IBusyTokenCallback>();
            Message = message;
        }

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            IsCompleted = true;
            foreach (var handler in _handlers)
                handler.OnCompleted(this);
            _handlers.Clear();
        }

        public bool IsCompleted { get; private set; }

        public object Message { get; private set; }

        public void Register(IBusyTokenCallback callback)
        {
            _handlers.Add(callback);
        }

        #endregion
    }
}
