#region Copyright

// ****************************************************************************
// <copyright file="Tracer.cs">
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

#define DEBUG
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using TraceLevel = MugenMvvmToolkit.Models.TraceLevel;

namespace MugenMvvmToolkit
{
    /// <summary>
    ///     Represents the default tracer.
    /// </summary>
    public class Tracer : ITracer, ITaskExceptionHandler
    {
        #region Fields

        /// <summary>
        ///     Gets the instance of <see cref="ITracer" />.
        /// </summary>
        internal static readonly Tracer Instance;

        #endregion

        #region Constructors

        static Tracer()
        {
            Instance = new Tracer();
            var isAttached = Debugger.IsAttached;
            TraceWarning = isAttached;
            TraceError = isAttached;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Tracer" /> class.
        /// </summary>
        protected Tracer()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the value that indicates that tracer should trace information messages.
        /// </summary>
        public static bool TraceInformation { get; set; }

        /// <summary>
        ///     Gets the value that indicates that tracer should trace warning messages.
        /// </summary>
        public static bool TraceWarning { get; set; }

        /// <summary>
        ///     Gets the value that indicates that tracer should trace error messages.
        /// </summary>
        public static bool TraceError { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Updates information about view-models.
        /// </summary>
        public static void TraceViewModel(AuditAction auditAction, IViewModel viewModel)
        {
            Action<AuditAction, IViewModel> handler = TraceViewModelHandler;
            if (handler != null)
                handler(auditAction, viewModel);
            ServiceProvider.Tracer.TraceViewModel(auditAction, viewModel);
        }

        /// <summary>
        ///     Writes an info message to the default tracer.
        /// </summary>
        public static void Info(string message)
        {
            if (TraceInformation)
                ServiceProvider.Tracer.Trace(TraceLevel.Information, message);
        }

        /// <summary>
        ///     Writes a warning message to the default tracer.
        /// </summary>
        public static void Warn(string message)
        {
            if (TraceWarning)
                ServiceProvider.Tracer.Trace(TraceLevel.Warning, message);
        }

        /// <summary>
        ///     Writes an error message to the default tracer.
        /// </summary>
        public static void Error(string message)
        {
            if (TraceError)
                ServiceProvider.Tracer.Trace(TraceLevel.Error, message);
        }

        /// <summary>
        ///     Writes an info message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            if (TraceInformation)
                ServiceProvider.Tracer.Trace(TraceLevel.Information, format, args);
        }

        /// <summary>
        ///     Writes a warning message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Warn(string format, params object[] args)
        {
            if (TraceWarning)
                ServiceProvider.Tracer.Trace(TraceLevel.Warning, format, args);
        }

        /// <summary>
        ///     Writes an error message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            if (TraceError)
                ServiceProvider.Tracer.Trace(TraceLevel.Error, format, args);
        }

        /// <summary>
        ///     Returns value that indicates that tracer can trace the level.
        /// </summary>
        public static bool CanTrace(TraceLevel level)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    return TraceInformation;
                case TraceLevel.Warning:
                    return TraceWarning;
                case TraceLevel.Error:
                    return TraceError;
                default:
                    throw ExceptionManager.EnumOutOfRange("level", level);
            }
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        protected virtual void TraceInternal(TraceLevel level, string message)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    Debug.WriteLine(message);
                    break;
                case TraceLevel.Warning:
                    Debug.WriteLine(message);
                    break;
                case TraceLevel.Error:
                    Debug.WriteLine(message);
                    break;
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Occurs on updates information about view-models.
        /// </summary>
        public static event Action<AuditAction, IViewModel> TraceViewModelHandler;

        #endregion

        #region Implementation of ITracer

        /// <summary>
        ///     Updates information about view-models.
        /// </summary>
        void ITracer.TraceViewModel(AuditAction auditAction, IViewModel viewModel)
        {
            TraceLevel traceLevel = auditAction == AuditAction.Finalized ? TraceLevel.Warning : TraceLevel.Information;
            if (!CanTrace(traceLevel))
                return;
            var displayName = viewModel as IHasDisplayName;
            if (displayName == null)
                Trace(traceLevel, "{0} ({1}) - {2};", viewModel.GetType(),
                    viewModel.GetHashCode().ToString(CultureInfo.InvariantCulture), auditAction);
            else
                Trace(traceLevel, "{0} (Hash - {1}; DisplayName - {2};) - {3}", viewModel.GetType(),
                    viewModel.GetHashCode().ToString(CultureInfo.InvariantCulture), displayName.DisplayName, auditAction);
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="message">The message to write.</param>
        public void Trace(TraceLevel level, string message)
        {
            if (CanTrace(level))
                TraceInternal(level, level + ": " + message);
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="format">The message to write.</param>
        /// <param name="args">The string format members.</param>
        public void Trace(TraceLevel level, string format, params object[] args)
        {
            if (CanTrace(level))
                TraceInternal(level, level + ": " + string.Format(format, args));
        }

        #endregion

        #region Implementation of ITaskExceptionHandler

        /// <summary>
        ///     Handles an exception.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="task">The task that throws an exception.</param>
        void ITaskExceptionHandler.Handle(object sender, Task task)
        {
            if (task.Exception != null)
                Error("Task exception handler - sender: {0}, exception: {1}", sender, task.Exception.Flatten(true));
        }

        #endregion
    }
}