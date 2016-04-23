#region Copyright

// ****************************************************************************
// <copyright file="ApplicationSettings.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit
{
    public static class ApplicationSettings
    {
        #region Fields

        public const string DataContractNamespace = "http://schemas.mugenmvvmtoolkit.com";
        public const string AssemblyVersion = "5.2.0.0";
        public const string AssemblyCopyright = "Copyright (c) 2012-2016 Vyacheslav Volkov";
        public const string AssemblyCompany = "Vyacheslav Volkov";

        #endregion

        #region Constructors

        static ApplicationSettings()
        {
            SetDefaultValues();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Action<RelayCommandBase, EventHandler> AddCanExecuteChangedEvent { get; set; }

        [CanBeNull]
        public static Action<RelayCommandBase, EventHandler> RemoveCanExecuteChangedEvent { get; set; }

        public static ExecutionMode PropertyChangeExecutionMode { get; set; }

        public static ObservationMode ViewModelObservationMode { get; set; }

        public static bool HandleTaskExceptionBusyIndicator { get; set; }

        public static ExecutionMode CommandCanExecuteMode { get; set; }

        public static CommandExecutionMode CommandExecutionMode { get; set; }

        public static int NotificationCollectionBatchSize { get; set; }

        public static bool MultiViewModelCloseViewModelsOnClose { get; set; }

        public static bool MultiViewModelDisposeViewModelOnRemove { get; set; }

        public static bool GridViewModelEnableSelectableInterface { get; set; }

        public static bool ViewMappingProviderDisableAutoRegistration { get; set; }

        public static bool SerializerDisableAutoRegistration { get; set; }

#if !NONOTIFYDATAERROR
        public static string GetAllErrorsIndexerProperty { get; set; }
#endif

        #endregion

        #region Methods

        internal static void SetDefaultValues()
        {
#if !NONOTIFYDATAERROR
            GetAllErrorsIndexerProperty = "all";
#endif
            MultiViewModelDisposeViewModelOnRemove = true;
            PropertyChangeExecutionMode = ExecutionMode.AsynchronousOnUiThread;
            ViewModelObservationMode = ObservationMode.ParentObserveChild;
            HandleTaskExceptionBusyIndicator = true;
            CommandCanExecuteMode = ExecutionMode.AsynchronousOnUiThread;
            CommandExecutionMode = CommandExecutionMode.CanExecuteBeforeExecute;
            NotificationCollectionBatchSize = 100;
        }

        #endregion
    }
}
