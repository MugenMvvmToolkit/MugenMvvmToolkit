#region Copyright

// ****************************************************************************
// <copyright file="TracerEx.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using System.Diagnostics;
using MugenMvvmToolkit.Attributes;
using TraceLevel = MugenMvvmToolkit.Models.TraceLevel;

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
            TraceWarning = Debugger.IsAttached;
            TraceError = true;
        }

        [Preserve(Conditional = true)]
        public TracerEx()
        {
        }

        #endregion

        #region Overrides of Tracer

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
