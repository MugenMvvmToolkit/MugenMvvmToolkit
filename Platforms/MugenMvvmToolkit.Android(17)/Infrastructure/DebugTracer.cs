#region Copyright
// ****************************************************************************
// <copyright file="DebugTracer.cs">
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
using Android.Util;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class DebugTracer : ITracer
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DebugTracer" /> class.
        /// </summary>
        public DebugTracer()
        {
            Tag = "MugenMvvmApp";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value that indicates that tracer should log events.
        /// </summary>
        public bool EnableLog { get; set; }

        /// <summary>
        /// Gets or sets the tag of log.
        /// </summary>
        public string Tag { get; set; }

        #endregion

        #region Implementation of ITracer

        /// <summary>
        ///     Updates information about view-models.
        /// </summary>
        public virtual void TraceViewModel(AuditAction auditAction, IViewModel viewModel)
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
        public virtual void Trace(TraceLevel level, string message)
        {
            message = level + ": " + message;
            switch (level)
            {
                case TraceLevel.Information:
                    System.Diagnostics.Trace.TraceInformation(message);
                    if (EnableLog)
                        Log.Info(Tag, message);
                    break;
                case TraceLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(message);
                    if (EnableLog)
                        Log.Warn(Tag, message);
                    break;
                case TraceLevel.Error:
                    System.Diagnostics.Trace.TraceError(message);
                    if (EnableLog)
                        Log.Error(Tag, message);
                    break;
            }
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="format">The message to write.</param>
        /// <param name="args">The string format members.</param>
        public virtual void Trace(TraceLevel level, string format, params object[] args)
        {
            Trace(level, string.Format(format, args));
        }

        #endregion
    }
}