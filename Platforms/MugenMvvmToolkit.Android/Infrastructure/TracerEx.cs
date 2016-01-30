#region Copyright

// ****************************************************************************
// <copyright file="TracerEx.cs">
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

using System.Diagnostics;
using Android.Util;
using TraceLevel = MugenMvvmToolkit.Models.TraceLevel;

#if XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Android.Infrastructure
#else
namespace MugenMvvmToolkit.Android.Infrastructure
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

        public TracerEx()
        {
            Tag = "MugenMvvmApp";
        }

        #endregion

        #region Properties

        public string Tag { get; set; }

        #endregion

        #region Overrides of Tracer

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
