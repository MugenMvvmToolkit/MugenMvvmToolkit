#region Copyright
// ****************************************************************************
// <copyright file="Tracer.cs">
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
#define DEBUG
using System.Diagnostics;
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
    public sealed class Tracer : ITracer, ITaskExceptionHandler
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
            TraceFinalized = Debugger.IsAttached;
        }

        private Tracer()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value that is responsible to trace finalized items.
        /// </summary>
        public static bool TraceFinalized { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Writes an info message to the default tracer.
        /// </summary>
        public static void Info(string message)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Information, message);
        }

        /// <summary>
        /// Writes a warning message to the default tracer.
        /// </summary>
        public static void Warn(string message)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Warning, message);
        }

        /// <summary>
        /// Writes an error message to the default tracer.
        /// </summary>
        public static void Error(string message)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Error, message);
        }

        /// <summary>
        /// Writes an info message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Info(string format, params object[] args)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Information, format, args);
        }

        /// <summary>
        /// Writes a warning message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Warn(string format, params object[] args)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Warning, format, args);
        }

        /// <summary>
        /// Writes an error message to the default tracer.
        /// </summary>
        [StringFormatMethod("format")]
        public static void Error(string format, params object[] args)
        {
            ServiceProvider.Tracer.Trace(TraceLevel.Error, format, args);
        }

        /// <summary>
        /// Writes information about an item.
        /// </summary>
        public static void Finalized(object item, string message = null)
        {
            if (TraceFinalized)
                Warn("Finalized - {0} ({1}); {2}", item.GetType(), item.GetHashCode().ToString(), message);
        }
        
        #endregion

        #region Implementation of ITracer

        /// <summary>
        ///     Updates information about view-models.
        /// </summary>
        public void TraceViewModel(AuditAction auditAction, IViewModel viewModel)
        {
            var hasDisplayName = viewModel as IHasDisplayName;
            var traceLevel = auditAction == AuditAction.Finalized ? TraceLevel.Warning : TraceLevel.Information;
            if (hasDisplayName == null)
                Trace(traceLevel,
                    string.Format("{0} ({1}) - {2}", viewModel.GetType(), viewModel.GetHashCode().ToString(), auditAction));
            else
                Trace(traceLevel,
                    string.Format("{0} (Hash - {1}; DisplayName - {2};) - {3}", viewModel.GetType(),
                        viewModel.GetHashCode().ToString(), hasDisplayName.DisplayName, auditAction));
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="message">The message to write.</param>
        public void Trace(TraceLevel level, string message)
        {
            if (!Debugger.IsAttached)
                return;
            message = level + ": " + message;
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

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="format">The message to write.</param>
        /// <param name="args">The string format members.</param>
        public void Trace(TraceLevel level, string format, params object[] args)
        {
            Trace(level, string.Format(format, args));
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