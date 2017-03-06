#region Copyright

// ****************************************************************************
// <copyright file="ConsoleTracer.cs">
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

        public void TraceViewModel(ViewModelLifecycleType lifecycleType, IViewModel viewModel)
        {
            Debug.WriteLine("{0}: {1}", lifecycleType, viewModel);
        }

        public void Trace(TraceLevel level, string message)
        {
            Debug.WriteLine("{0}: {1}", level, message);
        }

        public void Trace(TraceLevel level, string format, params object[] args)
        {
            Debug.WriteLine("{0}: {1}", level, string.Format(format, args));
        }

        #endregion
    }
}
