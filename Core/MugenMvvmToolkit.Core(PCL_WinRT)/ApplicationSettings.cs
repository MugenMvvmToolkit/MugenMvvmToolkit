#region Copyright

// ****************************************************************************
// <copyright file="ApplicationSettings.cs">
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
using System.Collections.Generic;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
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
        ///     Gets the data contract namespace.
        /// </summary>
        public const string DataContractNamespace = "http://schemas.mugenmvvmtoolkit.com";

        /// <summary>
        ///     Gets the assembly version.
        /// </summary>
        public const string AssemblyVersion = "4.3.0.0";

        /// <summary>
        ///     Gets the assembly copyright.
        /// </summary>
        public const string AssemblyCopyright = "Copyright (c) 2012-2015 Vyacheslav Volkov";

        /// <summary>
        ///     Gets the assembly company.
        /// </summary>
        public const string AssemblyCompany = "Vyacheslav Volkov";

        #endregion

        #region Constructors

        static ApplicationSettings()
        {
            SetDefaultValues();
        }

        #endregion

        #region Properties

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
        ///     Gets or sets the delegate that allows to convert non serializable value to serializable.
        /// </summary>
        [CanBeNull]
        public static Func<object, object> SaveValueState { get; set; }

        /// <summary>
        ///     Gets or sets the delegate that allows to convert from serializable value to original.
        /// </summary>
        [CanBeNull]
        public static Func<object, IDictionary<Type, object>, ICollection<IViewModel>, object> RestoreValueState { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>SynchronizedNotifiableCollection</c> by default, if not set explicitly.
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
        ///     Gets the value that is responsible to initialize the IocContainer using the IocContainer of parent view model.
        /// </summary>
        public static IocContainerCreationMode IocContainerCreationMode { get; set; }

        /// <summary>
        ///     Responsible for handling errors in the WithBusyIndicator method.
        ///     If true errors will be processed using the <see cref="ITaskExceptionHandler" /> interface; otherwise false.
        /// </summary>
        public static bool HandleTaskExceptionBusyIndicator { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>RaiseCanExecuteChanged</c> method in <c>IRelayCommand</c> by default.
        /// </summary>
        public static ExecutionMode CommandCanExecuteMode { get; set; }

        /// <summary>
        ///     Specifies the execution mode for <c>Execute</c> method in <c>IRelayCommand</c> by default.
        /// </summary>
        public static CommandExecutionMode CommandExecutionMode { get; set; }

#if !NONOTIFYDATAERROR
        /// <summary>
        ///     Gets or sets the value that is used to retrieve entity-level errors by <see cref="ValidatableViewModel.this"/>. Default value is <c>all</c>
        /// </summary>
        public static string GetAllErrorsIndexerProperty { get; set; }
#endif

        #endregion

        #region Methods

        internal static void SetDefaultValues()
        {
#if !NONOTIFYDATAERROR
            GetAllErrorsIndexerProperty = "all";
#endif
            NotificationCollectionMode =
                NotificationCollectionMode.CollectionIntefaceUseNotificationValue |
                NotificationCollectionMode.OnlyOnUiThread;
            SynchronizedCollectionExecutionMode = ExecutionMode.AsynchronousOnUiThread;
            PropertyChangeExecutionMode = ExecutionMode.AsynchronousOnUiThread;
            ViewModelObservationMode = ObservationMode.ParentObserveChild;
            HandleTaskExceptionBusyIndicator = true;
            CommandCanExecuteMode = ExecutionMode.AsynchronousOnUiThread;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            IocContainerCreationMode = IocContainerCreationMode.Mixed;
        }

        #endregion
    }
}