#region Copyright
// ****************************************************************************
// <copyright file="Enums.cs">
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
using System.Runtime.Serialization;

namespace MugenMvvmToolkit.Models
{
    [Flags]
    public enum MemberFlags
    {
        Static = 1,
        Instance = 2,
        Public = 4,
        NonPublic = 8,
    }

    /// <summary>
    ///     Represents the toast duration.
    /// </summary>
    public enum ToastDuration
    {
        /// <summary>
        /// Show the view or text notification for a short period of time. This time could be user-definable. This is the default.
        /// </summary>
        Short = 0,

        /// <summary>
        /// Show the view or text notification for a long period of time. This time could be user-definable.
        /// </summary>
        Long = 1
    }

    /// <summary>
    ///     Represents the toast position.
    /// </summary>
    public enum ToastPosition
    {
        /// <summary>
        ///     Toast is displayed at the bottom of the screen.
        /// </summary>
        Bottom = 0,

        /// <summary>
        ///     Toast is displayed at the center of the screen.
        /// </summary>
        Center = 1,

        /// <summary>
        ///     Toast is displayed at top of the screen.
        /// </summary>
        Top = 2
    }

    /// <summary>
    ///     Represents the enum that uses by notification collection.
    /// </summary>
    [Flags]
    public enum NotificationCollectionMode
    {
        /// <summary>
        ///     Both methods of interfaces return the real value of count.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The ICollection interface returns the notification values.
        /// </summary>
        CollectionIntefaceUseNotificationValue = 1,

        /// <summary>
        ///     The ICollection{T} interface returns the notification values.
        /// </summary>
        GenericCollectionInterfaceUseNotificationValue = 2,

        /// <summary>
        ///     The flag indicates that notification count will be returned only in UI thread.
        /// </summary>
        OnlyOnUiThread = 4
    }

    /// <summary>
    ///     Specifies the execution mode.
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        ///     Executes an action in the current thread.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Executes an action in Ui thread synchronous.
        /// </summary>
        SynchronousOnUiThread = 1,

        /// <summary>
        ///     Executes an action asynchronous.
        /// </summary>
        Asynchronous = 2,

        /// <summary>
        ///     Executes an action in Ui thread asynchronous.
        /// </summary>
        AsynchronousOnUiThread = 3
    }

    /// <summary>
    ///     Specifies the observation mode.
    /// </summary>
    [Flags]
    public enum ObservationMode
    {
        /// <summary>
        ///     None observe.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Parent view model observes child view model.
        /// </summary>
        ParentObserveChild = 1,

        /// <summary>
        ///     Child view model observes parent view model.
        /// </summary>
        ChildObserveParent = 2,

        /// <summary>
        ///     Child view model observes parent view model and parent view model observes child view model..
        /// </summary>
        Both = ParentObserveChild | ChildObserveParent
    }

    /// <summary>
    ///     Specifies the ioc container creation mode.
    /// </summary>
    [Flags]
    public enum IocContainerCreationMode
    {

        /// <summary>
        ///     The view model uses the default application IocContainer.
        /// </summary>
        Application = 1,

        /// <summary>
        ///     The view model uses the IocContainer from parent view model.
        /// </summary>
        ParentViewModel = 2,

        /// <summary>
        ///     The view model combines the application and parent view model container in the one.
        /// </summary>
        Mixed = ParentViewModel | Application
    }

    /// <summary>
    ///     Specifies the hadnle mode.
    /// </summary>
    [Flags]
    public enum HandleMode
    {
        /// <summary>
        ///     None handle
        /// </summary>
        None = 0,

        /// <summary>
        ///     Handles and processes a message.
        /// </summary>
        Handle = 1,

        /// <summary>
        ///     Handles and notifies observers about a message.
        /// </summary>
        NotifyObservers = 2,

        /// <summary>
        ///     Handles and processes a message and notifies observers about the message.
        /// </summary>
        HandleAndNotifyObservers = Handle | NotifyObservers
    }

    /// <summary>
    ///     Represents the audit action information.
    /// </summary>
    public enum AuditAction
    {
        /// <summary>
        ///     Create action.
        /// </summary>
        Created = 1,

        /// <summary>
        ///     Load action
        /// </summary>
        Initialized = 2,

        /// <summary>
        ///     Dispose action.
        /// </summary>
        Disposed = 3,

        /// <summary>
        ///     Finalize action.
        /// </summary>
        Finalized = 4,

        /// <summary>
        ///     Restore action.
        /// </summary>
        Restored = 5
    }

    /// <summary>
    ///     Specifies the buttons that are displayed on a message box. Used as an argument of the IMessageBox.Show methods.
    /// </summary>
    public enum MessageButton
    {
        /// <summary>
        ///     Ok button.
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     Ok and cancel buttons.
        /// </summary>
        OkCancel = 1,

        /// <summary>
        ///     Yes and no buttons.
        /// </summary>
        YesNo = 2,

        /// <summary>
        ///     Yes, no and cancel buttons.
        /// </summary>
        YesNoCancel = 3,

        /// <summary>
        ///     Abort, retry and ignore buttons.
        /// </summary>
        AbortRetryIgnore = 4,

        /// <summary>
        ///     Retry and cancel buttons.
        /// </summary>
        RetryCancel = 5
    }

    /// <summary>
    ///     Specifies the icon that is displayed by a message box.
    /// </summary>
    public enum MessageImage
    {
        /// <summary>
        ///     None image.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Asterisk image.
        /// </summary>
        Asterisk = 1,

        /// <summary>
        ///     Error image.
        /// </summary>
        Error = 2,

        /// <summary>
        ///     Exclamation image.
        /// </summary>
        Exclamation = 3,

        /// <summary>
        ///     Hand image.
        /// </summary>
        Hand = 4,

        /// <summary>
        ///     Information image.
        /// </summary>
        Information = 5,

        /// <summary>
        ///     Question image.
        /// </summary>
        Question = 6,

        /// <summary>
        ///     Stop image.
        /// </summary>
        Stop = 7,

        /// <summary>
        ///     Warning image.
        /// </summary>
        Warning = 8
    }

    /// <summary>
    ///     Specifies which message box button that a user clicks.
    /// </summary>
    public enum MessageResult
    {
        /// <summary>
        ///     None result.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Ok result.
        /// </summary>
        Ok = 1,

        /// <summary>
        ///     Cancel result.
        /// </summary>
        Cancel = 2,

        /// <summary>
        ///     No result.
        /// </summary>
        No = 3,

        /// <summary>
        ///     Yes result.
        /// </summary>
        Yes = 4,

        /// <summary>
        ///     Abort result.
        /// </summary>
        Abort = 5,

        /// <summary>
        ///     Retry result.
        /// </summary>
        Retry = 6,

        /// <summary>
        ///     Ignore result.
        /// </summary>
        Ignore = 7
    }

    /// <summary>
    ///     Represents the enum that uses by <c>IRelayCommand</c> to call the <c>Execute</c> method.
    /// </summary>
    public enum CommandExecutionMode
    {
        /// <summary>
        ///     Call the <c>Execute</c> method without checks.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Call the <c>CanExecute</c> method before call the <c>Execute</c> method.
        /// </summary>
        CanExecuteBeforeExecute = 1,

        /// <summary>
        ///     Call the <c>CanExecute</c> method before call the <c>Execute</c> method and throw an exception if needed.
        /// </summary>
        CanExecuteBeforeExecuteWithException = 2
    }

    /// <summary>
    ///     Describes the priorities at which operations can be invoked by way of the <c>IDispatcher</c>.
    /// </summary>
    public enum OperationPriority
    {
        /// <summary>
        ///     Low priority.
        /// </summary>
        Low = -1,

        /// <summary>
        ///     Normal priority.
        /// </summary>
        Normal = 0,

        /// <summary>
        ///     High priority.
        /// </summary>
        High = 1,
    }

    /// <summary>
    ///     Represents the entity state.
    /// </summary>
    [Flags, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public enum EntityState
    {
        /// <summary>
        ///     Unchanged state.
        /// </summary>
        [EnumMember]
        Unchanged = 1,

        /// <summary>
        ///     Added state.
        /// </summary>
        [EnumMember]
        Added = 2,

        /// <summary>
        ///     Deleted state.
        /// </summary>
        [EnumMember]
        Deleted = 4,

        /// <summary>
        ///     Modified state.
        /// </summary>
        [EnumMember]
        Modified = 8,

        /// <summary>
        ///     Detached state.
        /// </summary>
        [EnumMember]
        Detached = 16
    }

    /// <summary>
    ///     Specifies the type of navigation.
    /// </summary>
    public enum NavigationMode
    {
        /// <summary>
        ///     Undefined navigation mode.
        /// </summary>
        Undefined = 0,

        /// <summary>
        ///     New navigation mode.
        /// </summary>
        New = 1,

        /// <summary>
        ///     Back navigation mode.
        /// </summary>
        Back = 2,

        /// <summary>
        ///     Forward navigation mode.
        /// </summary>
        Forward = 3,

        /// <summary>
        ///     Refresh navigation mode.
        /// </summary>
        Refresh = 4,

        /// <summary>
        ///     Reset navigation mode.
        /// </summary>
        Reset = 5
    }

    /// <summary>
    ///     Specifies the display trace level.
    /// </summary>
    public enum TraceLevel
    {
        /// <summary>
        ///     Information trace level.
        /// </summary>
        Information = 0,

        /// <summary>
        ///     Warning trace level.
        /// </summary>
        Warning = 1,

        /// <summary>
        ///     Error trace level.
        /// </summary>
        Error = 2
    }

    /// <summary>
    ///     Specifies the load type.
    /// </summary>
    [Flags]
    public enum LoadMode
    {
        /// <summary>
        ///     Desing mode.
        /// </summary>
        Design = 1,

        /// <summary>
        ///     Unit-test mode.
        /// </summary>
        UnitTest = 2,

        /// <summary>
        ///     Runtime mode.
        /// </summary>
        Runtime = 4,

        /// <summary>
        ///     All modes.
        /// </summary>
        All = Design | UnitTest | Runtime
    }

    /// <summary>
    ///     Specifies the handler result.
    /// </summary>
    public enum HandlerResult
    {
        /// <summary>
        ///     Indicates that message was handled.
        /// </summary>
        Handled = 1,

        /// <summary>
        ///     Indicates that message was ignored.
        /// </summary>
        Ignored = 2,

        /// <summary>
        ///     Indicates that handler has invalid state and should be removed.
        /// </summary>
        Invalid = 3
    }
}