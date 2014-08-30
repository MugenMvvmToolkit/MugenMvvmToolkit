#region Copyright
// ****************************************************************************
// <copyright file="Empty.cs">
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
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvmToolkit.Annotations;

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

            #endregion

            #region Constructors

            static Value()
            {
                ArrayInstance = new T[0];
            }

            #endregion
        }

        #endregion

        #region Fields

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
        /// Gets the empty weak reference.
        /// </summary>
        public static readonly WeakReference WeakReference;

        internal static readonly PropertyChangedEventArgs CountPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs NotificationCountPropertyChangedArgs;
        internal static readonly PropertyChangedEventArgs IndexerPropertyChangedArgs;

        #endregion

        #region Constructors

        static Empty()
        {
            WeakReference = new WeakReference(null, false);
            TrueTask = Extensions.FromResult(true);
            FalseTask = Extensions.FromResult(false);
            Task = FalseTask;
            CountPropertyChangedArgs = new PropertyChangedEventArgs("Count");
            NotificationCountPropertyChangedArgs = new PropertyChangedEventArgs("NotificationCount");
            IndexerPropertyChangedArgs = new PropertyChangedEventArgs("Item[]");
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

        #endregion
    }
}