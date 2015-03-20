#region Copyright

// ****************************************************************************
// <copyright file="Empty.cs">
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
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the empty values helper.
    /// </summary>
    public static class Empty
    {
        #region Nested types

        private static class Value<T>
        {
            #region Fields

            public static readonly T[] ArrayInstance;
            public static readonly Task<T> CanceledTaskField;


            #endregion

            #region Constructors

            static Value()
            {
                ArrayInstance = new T[0];
                var tcs = new TaskCompletionSource<T>();
                tcs.SetCanceled();
                CanceledTaskField = tcs.Task;
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        ///     Gets the boxed true value.
        /// </summary>
        public static readonly object TrueObject;

        /// <summary>
        ///     Gets the boxed false value.
        /// </summary>
        public static readonly object FalseObject;

        /// <summary>
        ///     Gets the completed task with true result.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static readonly Task<bool> TrueTask;

        /// <summary>
        ///     Gets the completed task with false result.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static readonly Task<bool> FalseTask;

        /// <summary>
        ///     Gets the completed task.
        /// </summary>
        [SuppressTaskBusyHandler]
        public static readonly Task Task;

        /// <summary>
        ///     Gets the empty weak reference.
        /// </summary>
        public static readonly WeakReference WeakReference;

        internal static readonly ManualResetEvent CompletedEvent;
        internal static readonly PropertyChangedEventArgs CountChangedArgs;
        internal static readonly PropertyChangedEventArgs NotificationCountChangedArgs;
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs IsNotificationsSuspendedChangedArgs;
        internal static readonly PropertyChangedEventArgs HasChangesChangedArgs;
        internal static readonly PropertyChangedEventArgs SelectedItemChangedArgs;
        internal static readonly PropertyChangedEventArgs HasErrorsChangedArgs;
        internal static readonly PropertyChangedEventArgs IsValidChangedArgs;
        internal static readonly PropertyChangedEventArgs IsBusyChangedArgs;
        internal static readonly PropertyChangedEventArgs BusyMessageChangedArgs;
        internal static readonly PropertyChangedEventArgs EmptyPropertyChangedArgs;
        internal static readonly DataErrorsChangedEventArgs EmptyDataErrorsChangedArgs;

        #endregion

        #region Constructors

        static Empty()
        {
            CompletedEvent = new ManualResetEvent(true);
            TrueObject = true;
            FalseObject = false;
            WeakReference = new WeakReference(null, false);
            TrueTask = ToolkitExtensions.FromResult(true);
            FalseTask = ToolkitExtensions.FromResult(false);
            Task = FalseTask;
            EmptyDataErrorsChangedArgs = new DataErrorsChangedEventArgs(string.Empty);
            EmptyPropertyChangedArgs = new PropertyChangedEventArgs(string.Empty);
            CountChangedArgs = new PropertyChangedEventArgs("Count");
            NotificationCountChangedArgs = new PropertyChangedEventArgs("NotificationCount");
            IndexerPropertyChangedArgs = new PropertyChangedEventArgs("Item[]");
            IsNotificationsSuspendedChangedArgs = new PropertyChangedEventArgs("IsNotificationsSuspended");
            HasChangesChangedArgs = new PropertyChangedEventArgs("HasChanges");
            SelectedItemChangedArgs = new PropertyChangedEventArgs("SelectedItem");
            HasErrorsChangedArgs = new PropertyChangedEventArgs("HasErrors");
            IsValidChangedArgs = new PropertyChangedEventArgs("IsValid");
            IsBusyChangedArgs = new PropertyChangedEventArgs("IsBusy");
            BusyMessageChangedArgs = new PropertyChangedEventArgs("BusyMessage");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the array instance.
        /// </summary>
        public static T[] Array<T>()
        {
            return Value<T>.ArrayInstance;
        }

        /// <summary>
        ///     Gets the canceled task.
        /// </summary>
        public static Task<T> CanceledTask<T>()
        {
            return Value<T>.CanceledTaskField;
        }

        /// <summary>
        ///     Converts a bool value to boxed value.
        /// </summary>
        public static object BooleanToObject(bool value)
        {
            if (value)
                return TrueObject;
            return FalseObject;
        }

        #endregion
    }
}