#region Copyright
// ****************************************************************************
// <copyright file="ITracer.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Interfaces
{
    /// <summary>
    ///     Represents the tracer interface.
    /// </summary>
    public interface ITracer
    {
        /// <summary>
        ///     Writes information about view-models.
        /// </summary>
        void TraceViewModel(AuditAction auditAction, [NotNull] IViewModel viewModel);

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="message">The message to write.</param>
        void Trace(TraceLevel level, string message);

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        /// <param name="level">The specified trace level.</param>
        /// <param name="format">The message to write.</param>
        /// <param name="args">The string format members.</param>
        [StringFormatMethod("format")]
        void Trace(TraceLevel level, string format, params object[] args);
    }
}