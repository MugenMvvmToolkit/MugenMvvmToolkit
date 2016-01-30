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
