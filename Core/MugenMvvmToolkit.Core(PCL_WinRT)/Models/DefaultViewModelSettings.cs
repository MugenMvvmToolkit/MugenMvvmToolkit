#region Copyright

// ****************************************************************************
// <copyright file="DefaultViewModelSettings.cs">
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

using System.Threading;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    public class DefaultViewModelSettings : DataContext, IViewModelSettings
    {
        #region Fields

        private object _defaultBusyMessage;
        private IDataContext _state;

        #endregion

        #region Constructors

        public DefaultViewModelSettings()
        {
            _defaultBusyMessage = string.Empty;
            DisposeCommands = true;
            HandleBusyMessageMode = HandleMode.Handle;
            EventExecutionMode = ExecutionMode.AsynchronousOnUiThread;
        }

        #endregion

        #region Implementation of IViewModelSettings

        public bool BroadcastAllMessages { get; set; }

        public bool DisposeCommands { get; set; }

        public HandleMode HandleBusyMessageMode { get; set; }

        public object DefaultBusyMessage
        {
            get { return _defaultBusyMessage; }
            set
            {
                Should.PropertyNotBeNull(value);
                _defaultBusyMessage = value;
            }
        }

        public ExecutionMode EventExecutionMode { get; set; }

        public IDataContext Metadata => this;

        public IDataContext State
        {
            get
            {
                if (_state == null)
                    Interlocked.CompareExchange(ref _state, new DataContext(), null);
                return _state;
            }
            protected set { _state = value; }
        }

        #endregion
    }
}
