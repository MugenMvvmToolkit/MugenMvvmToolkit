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

using Android.Util;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    public class TracerEx : Tracer
    {
        #region Constructors

        static TracerEx()
        {
            TraceWarning = true;
            TraceError = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TracerEx" /> class.
        /// </summary>
        public TracerEx()
        {
            Tag = "MugenMvvmApp";
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the tag of log.
        /// </summary>
        public string Tag { get; set; }

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
                    Log.Info(Tag, message);
                    break;
                case TraceLevel.Warning:
                    Log.Warn(Tag, message);
                    break;
                case TraceLevel.Error:
                    Log.Error(Tag, message);
                    break;
            }
        }

        #endregion
    }
}