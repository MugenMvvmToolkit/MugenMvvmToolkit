#region Copyright

// ****************************************************************************
// <copyright file="DefaultViewModelSettings.cs">
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

        private DefaultViewModelSettings(IDataContext metadata, IDataContext state)
            : base(metadata ?? Empty)
        {
            if (state != null)
                _state = new DataContext(state);
        }

        public DefaultViewModelSettings()
        {
            _defaultBusyMessage = string.Empty;
            DisposeIocContainer = true;
            DisposeCommands = true;
            HandleBusyMessageMode = HandleMode.Handle;
            EventExecutionMode = ExecutionMode.AsynchronousOnUiThread;
        }

        #endregion

        #region Implementation of IViewModelSettings

        public bool BroadcastAllMessages { get; set; }

        public bool DisposeIocContainer { get; set; }

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

        public IDataContext Metadata
        {
            get { return this; }
        }

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

        public virtual IViewModelSettings Clone()
        {
            return new DefaultViewModelSettings(this, _state)
            {
                DisposeCommands = DisposeCommands,
                DisposeIocContainer = DisposeIocContainer,
                HandleBusyMessageMode = HandleBusyMessageMode,
                EventExecutionMode = EventExecutionMode,
                DefaultBusyMessage = DefaultBusyMessage,
                BroadcastAllMessages = BroadcastAllMessages
            };
        }

        #endregion
    }
}
