#define DEBUG
using System.Diagnostics;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using TraceLevel = MugenMvvmToolkit.Models.TraceLevel;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class ConsoleTracer : ITracer
    {
        #region Implementation of ITracer

        /// <summary>
        ///     Writes information about view-models.
        /// </summary>
        public void TraceViewModel(AuditAction auditAction, IViewModel viewModel)
        {
            Debug.WriteLine("{0}: {1}", auditAction, viewModel);
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="message">The message to write.</param>
        public void Trace(TraceLevel level, string message)
        {
            Debug.WriteLine("{0}: {1}", level, message);
        }

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="format">The message to write.</param>
        /// <param name="args">The string format members.</param>
        public void Trace(TraceLevel level, string format, params object[] args)
        {
            Debug.WriteLine("{0}: {1}", level, string.Format(format, args));
        }

        #endregion
    }
}