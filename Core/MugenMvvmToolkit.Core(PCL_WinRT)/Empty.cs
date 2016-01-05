#region Copyright

// ****************************************************************************
// <copyright file="Empty.cs">
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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit
{
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

        public static readonly object TrueObject;
        public static readonly object FalseObject;
        public static readonly Task<bool> TrueTask;
        public static readonly Task<bool> FalseTask;
        public static readonly Task Task;
        public static readonly WeakReference WeakReference;

        internal static readonly NotifyCollectionChangedEventArgs ResetEventArgs;
        internal static readonly ManualResetEvent CompletedEvent;
        internal static readonly PropertyChangedEventArgs CountChangedArgs;
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
            CountChangedArgs = new PropertyChangedEventArgs(nameof(ICollection.Count));
            IndexerPropertyChangedArgs = new PropertyChangedEventArgs(ReflectionExtensions.IndexerName);
            IsNotificationsSuspendedChangedArgs = new PropertyChangedEventArgs(nameof(ISuspendNotifications.IsNotificationsSuspended));
            HasChangesChangedArgs = new PropertyChangedEventArgs(nameof(IEditableViewModel.HasChanges));
            SelectedItemChangedArgs = new PropertyChangedEventArgs(nameof(IGridViewModel.SelectedItem));
            HasErrorsChangedArgs = new PropertyChangedEventArgs(nameof(IValidatableViewModel.HasErrors));
            IsValidChangedArgs = new PropertyChangedEventArgs(nameof(IValidatableViewModel.IsValid));
            IsBusyChangedArgs = new PropertyChangedEventArgs(nameof(IViewModel.IsBusy));
            BusyMessageChangedArgs = new PropertyChangedEventArgs(nameof(IViewModel.BusyMessage));
            ResetEventArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        }

        #endregion

        #region Methods

        public static T[] Array<T>()
        {
            return Value<T>.ArrayInstance;
        }

        public static Task<T> CanceledTask<T>()
        {
            return Value<T>.CanceledTaskField;
        }

        public static object BooleanToObject(bool value)
        {
            if (value)
                return TrueObject;
            return FalseObject;
        }

        #endregion
    }
}
