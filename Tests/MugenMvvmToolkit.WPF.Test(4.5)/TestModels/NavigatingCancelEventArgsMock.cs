using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigatingCancelEventArgsMock : NavigatingCancelEventArgsBase
    {
        #region Constructors

        public NavigatingCancelEventArgsMock(NavigationMode mode, bool isCancelable, IDataContext context = null)
        {
            NavigationMode = mode;
            IsCancelable = isCancelable;
            Context = context;
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode { get; }

        public override bool IsCancelable { get; }

        public override IDataContext Context { get; }

        #endregion
    }
}