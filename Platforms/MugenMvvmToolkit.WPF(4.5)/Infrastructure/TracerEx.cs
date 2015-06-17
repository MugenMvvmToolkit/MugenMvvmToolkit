#region Copyright

// ****************************************************************************
// <copyright file="TracerEx.cs">
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

using MugenMvvmToolkit.Models;

#if WPF
namespace MugenMvvmToolkit.WPF.Infrastructure
#elif WINFORMS
namespace MugenMvvmToolkit.WinForms.Infrastructure
#endif

{
    public class TracerEx : Tracer
    {
        #region Constructors

        static TracerEx()
        {
            TraceWarning = true;
            TraceError = true;
        }

        #endregion

        #region Overrides of Tracer

        /// <summary>
        ///     Writes an informational message to the trace listeners.
        /// </summary>
        protected override void TraceInternal(TraceLevel level, string message)
        {
            switch (level)
            {
                case TraceLevel.Information:
                    System.Diagnostics.Trace.TraceInformation(message);
                    break;
                case TraceLevel.Warning:
                    System.Diagnostics.Trace.TraceWarning(message);
                    break;
                case TraceLevel.Error:
                    System.Diagnostics.Trace.TraceError(message);
                    break;
            }
        }

        #endregion
    }
}