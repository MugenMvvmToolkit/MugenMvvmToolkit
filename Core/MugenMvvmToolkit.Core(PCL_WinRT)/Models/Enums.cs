#region Copyright

// ****************************************************************************
// <copyright file="Enums.cs">
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

    public enum ToastDuration
    {
        Short = 0,
        Long = 1
    }

    public enum ToastPosition
    {
        Bottom = 0,
        Center = 1,
        Top = 2
    }

    [Flags]
    public enum NotificationCollectionMode
    {
        None = 0,
        CollectionIntefaceUseNotificationValue = 1,
        GenericCollectionInterfaceUseNotificationValue = 2,
        OnlyOnUiThread = 4
    }

    public enum ExecutionMode
    {
        None = 0,
        SynchronousOnUiThread = 1,
        Asynchronous = 2,
        AsynchronousOnUiThread = 3
    }

    [Flags]
    public enum ObservationMode
    {
        None = 0,
        ParentObserveChild = 1,
        ChildObserveParent = 2,
        Both = ParentObserveChild | ChildObserveParent
    }

    [Flags]
    public enum HandleMode
    {
        None = 0,
        Handle = 1,
        NotifySubscribers = 2,
        HandleAndNotifySubscribers = Handle | NotifySubscribers
    }

    public enum ViewModelLifecycleType
    {
        Created = 1,
        Initialized = 2,
        Disposed = 3,
        Finalized = 4,
        Restored = 5
    }

    public enum MessageButton
    {
        Ok = 0,
        OkCancel = 1,
        YesNo = 2,
        YesNoCancel = 3,
        AbortRetryIgnore = 4,
        RetryCancel = 5
    }

    public enum MessageImage
    {
        None = 0,
        Asterisk = 1,
        Error = 2,
        Exclamation = 3,
        Hand = 4,
        Information = 5,
        Question = 6,
        Stop = 7,
        Warning = 8
    }

    public enum MessageResult
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        No = 3,
        Yes = 4,
        Abort = 5,
        Retry = 6,
        Ignore = 7
    }

    public enum CommandExecutionMode
    {
        None = 0,
        CanExecuteBeforeExecute = 1,
        CanExecuteBeforeExecuteWithException = 2
    }

    public enum OperationPriority
    {
        Low = -1,
        Normal = 0,
        High = 1,
    }

    [Flags, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public enum EntityState
    {
        [EnumMember]
        Unchanged = 1,

        [EnumMember]
        Added = 2,

        [EnumMember]
        Deleted = 4,

        [EnumMember]
        Modified = 8,

        [EnumMember]
        Detached = 16
    }

    public enum NavigationMode
    {
        Undefined = 0,
        New = 1,
        Back = 2,
        Forward = 3,
        Refresh = 4,
        Reset = 5
    }

    public enum TraceLevel
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }

    [Flags]
    public enum LoadMode
    {
        Design = 1,
        UnitTest = 2,
        Runtime = 4,
        RuntimeDebug = Runtime | 8,
        RuntimeRelease = Runtime | 16,
        All = Design | UnitTest | RuntimeDebug | RuntimeRelease
    }

    public enum HandlerResult
    {
        Handled = 1,
        Ignored = 2,
        Invalid = 3
    }
}
