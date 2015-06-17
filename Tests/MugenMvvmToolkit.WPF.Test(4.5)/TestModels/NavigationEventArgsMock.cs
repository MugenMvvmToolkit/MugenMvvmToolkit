using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Silverlight.Models.EventArg;
using MugenMvvmToolkit.WinRT.Models.EventArg;
using MugenMvvmToolkit.WPF.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigationEventArgsMock : NavigationEventArgsBase
    {
        #region Fields

        private readonly object _content;
        private readonly NavigationMode _mode;

        #endregion

        #region Constructors

        public NavigationEventArgsMock(object content, NavigationMode mode)
        {
            _content = content;
            _mode = mode;
        }

        #endregion

        #region Overrides of NavigationEventArgsBase

        public override object Content
        {
            get { return _content; }
        }

        public override NavigationMode Mode
        {
            get { return _mode; }
        }

        #endregion
    }
}