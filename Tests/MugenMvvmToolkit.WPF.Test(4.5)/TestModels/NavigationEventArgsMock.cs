using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigationEventArgsMock : NavigationEventArgsBase
    {
        #region Constructors

        public NavigationEventArgsMock(object content, NavigationMode mode, IDataContext context = null)
        {
            Content = content;
            NavigationMode = mode;
            Context = context;
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content { get; }

        public override NavigationMode NavigationMode { get; }

        public override IDataContext Context { get; }

        #endregion
    }
}