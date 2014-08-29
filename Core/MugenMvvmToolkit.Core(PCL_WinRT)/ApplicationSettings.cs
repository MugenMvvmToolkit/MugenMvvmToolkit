#region Copyright
// ****************************************************************************
// <copyright file="ApplicationSettings.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the static class for application settings.
    /// </summary>
    public static class ApplicationSettings
    {
        #region Fields

        /// <summary>
        /// Gets the data contract namespace.
        /// </summary>
        public const string DataContractNamespace = "http://schemas.mugenmvvmtoolkit.com";
        private static IViewModelSettings _viewModelSettings;
        private static PlatformInfo _platform;

        #endregion

        #region Constructors

        static ApplicationSettings()
        {
            SetDefaultValues();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        [NotNull]
        public static PlatformInfo Platform
        {
            get
            {
                if (_platform == null)
                    return PlatformInfo.Unknown;
                return _platform;
            }
            set { _platform = value; }
        }

        /// <summary>
        ///     Gets or sets the default view-model settings.
        /// </summary>
        [NotNull]
        public static IViewModelSettings ViewModelSettings
        {
            get
            {
                if (_viewModelSettings == null)
                    Interlocked.CompareExchange(ref _viewModelSettings, new DefaultViewModelSettings(), null);
                return _viewModelSettings;
            }
            set { _viewModelSettings = value; }
        }

        /// <summary>
        ///     Gets the delegate that is responsible to add can execute changed event to external provider.
        /// </summary>
        [CanBeNull]
        public static Action<RelayCommandBase, EventHandler> AddCanExecuteChangedEvent { get; set; }

        /// <summary>
        ///     Gets the delegate that is responsible to remove can execute changed event from external provider.
        /// </summary>
        [CanBeNull]
        public static Action<RelayCommandBase, EventHandler> RemoveCanExecuteChangedEvent { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>SyncronizedNotifiableCollection</c> by default, if not set explicitly.
        /// </summary>
        public static ExecutionMode SynchronizedCollectionExecutionMode { get; set; }

        /// <summary>
        ///     Specifies the mode for <c>NotifiableCollection</c> by default, if not set explicitly.
        /// </summary>
        public static NotificationCollectionMode NotificationCollectionMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for raise property changed event in <c>NotifyPropertyChangedBase</c> by default, if
        ///     not set explicitly.
        /// </summary>
        public static ExecutionMode PropertyChangeExecutionMode { get; set; }

        /// <summary>
        ///     Gets or sets the value that responsible for listen messages in child view models, if not set explicitly.
        /// </summary>
        public static ObservationMode ViewModelObservationMode { get; set; }

        /// <summary>
        ///     Responsible for handling errors in the WithBusyIndicator method.
        ///     If true errors will be processed using the <see cref="ITaskExceptionHandler" /> interface; otherwise false.
        /// </summary>
        public static bool HandleTaskExceptionWithBusyIndicator { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>RaiseCanExecuteChanged</c> method in <c>IRelayCommand</c> by default.
        /// </summary>
        public static ExecutionMode CommandCanExecuteMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>Execute</c> method in <c>IRelayCommand</c> by default.
        /// </summary>
        public static CommandExecutionMode CommandExecutionMode { get; set; }

        #endregion

        #region Methods

        internal static void SetDefaultValues()
        {
            NotificationCollectionMode =
                NotificationCollectionMode.CollectionIntefaceUseNotificationValue |
                NotificationCollectionMode.OnlyOnUiThread;
            SynchronizedCollectionExecutionMode = ExecutionMode.AsynchronousOnUiThread;
            PropertyChangeExecutionMode = ExecutionMode.AsynchronousOnUiThread;
            ViewModelObservationMode = ObservationMode.ParentObserveChild;
            HandleTaskExceptionWithBusyIndicator = true;
            CommandCanExecuteMode = ExecutionMode.AsynchronousOnUiThread;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
        }

        #endregion
    }
}