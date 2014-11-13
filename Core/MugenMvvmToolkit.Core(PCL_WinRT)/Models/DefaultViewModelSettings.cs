#region Copyright
// ****************************************************************************
// <copyright file="DefaultViewModelSettings.cs">
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
using System.Threading;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models.Messages;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the default view-model settings.
    /// </summary>
    public class DefaultViewModelSettings : IViewModelSettings
    {
        #region Fields

        private object _defaultBusyMessage;
        private IDataContext _metadata;
        private IDataContext _state;
        private object _validationBusyMessage;

        #endregion

        #region Constructor

        private DefaultViewModelSettings(IDataContext metadata, IDataContext state)
        {
            if (_metadata != null)
                _metadata = new DataContext(metadata);
            if (state != null)
                _state = new DataContext(state);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultViewModelSettings" /> class.
        /// </summary>
        public DefaultViewModelSettings()
        {
            _defaultBusyMessage = string.Empty;
            _validationBusyMessage = string.Empty;
            DisposeIocContainer = true;
            DisposeCommands = true;
            BroadcastAllMessages = true;
            HandleBusyMessageMode = HandleMode.HandleAndNotifyObservers;
            EventExecutionMode = ExecutionMode.AsynchronousOnUiThread;
        }

        #endregion

        #region Implementation of IViewModelSettings

        /// <summary>
        ///     Gets or sets property, that is responsible for broadcast all messages through all view models in chain.
        /// </summary>
        public bool BroadcastAllMessages { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose container when the view model disposing.
        /// </summary>
        public bool DisposeIocContainer { get; set; }

        /// <summary>
        ///     Gets or sets property, that is responsible for auto dispose all command when the view model disposing.
        /// </summary>
        public bool DisposeCommands { get; set; }

        /// <summary>
        ///     Gets or sets the value that is responsible for listen busy messages
        ///     see the <see cref="BeginBusyMessage" /> and the <see cref="EndBusyMessage" />.
        /// </summary>
        public HandleMode HandleBusyMessageMode { get; set; }

        /// <summary>
        ///     Gets or sets value that will be displayed when the BeginIsBusy method will be invoked without a message.
        /// </summary>
        public object DefaultBusyMessage
        {
            get { return _defaultBusyMessage; }
            set
            {
                Should.PropertyBeNotNull(value);
                _defaultBusyMessage = value;
            }
        }

        /// <summary>
        ///     Gets or sets value that will be displayed when the BeginIsBusy method will be invoked without a message.
        /// </summary>
        public object ValidationBusyMessage
        {
            get { return _validationBusyMessage; }
            set
            {
                Should.PropertyBeNotNull(value);
                _validationBusyMessage = value;
            }
        }

        /// <summary>
        ///     Specifies the execution mode for invoke events (<c>ErrorsChanged</c>, <c>SelectedItemChanged</c>, etc).
        /// </summary>
        public ExecutionMode EventExecutionMode { get; set; }

        /// <summary>
        ///     Gets the data context of current view model.
        /// </summary>
        public IDataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    Interlocked.CompareExchange(ref _metadata, new DataContext(), null);
                return _metadata;
            }
            protected set { _metadata = value; }
        }

        /// <summary>
        ///     Gets the serializable state of view model.
        /// </summary>
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

        /// <summary>
        ///     Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        ///     A new object that is a copy of this instance.
        /// </returns>
        public virtual IViewModelSettings Clone()
        {
            return new DefaultViewModelSettings(_metadata, _state)
            {
                DisposeCommands = DisposeCommands,
                DisposeIocContainer = DisposeIocContainer,
                HandleBusyMessageMode = HandleBusyMessageMode,
                EventExecutionMode = EventExecutionMode,
                DefaultBusyMessage = DefaultBusyMessage,
                ValidationBusyMessage = ValidationBusyMessage,
                BroadcastAllMessages = BroadcastAllMessages
            };
        }

        #endregion
    }
}