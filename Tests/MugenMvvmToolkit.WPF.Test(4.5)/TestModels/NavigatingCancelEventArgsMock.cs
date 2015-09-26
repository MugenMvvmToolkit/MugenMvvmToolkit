using System;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Silverlight.Models.EventArg;
using MugenMvvmToolkit.WinRT.Models.EventArg;
using MugenMvvmToolkit.WPF.Models.EventArg;

namespace MugenMvvmToolkit.Test.TestModels
{
    public class NavigatingCancelEventArgsMock : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly bool _isCancelable;
        private readonly NavigationMode _mode;

        #endregion

        #region Constructors

        public NavigatingCancelEventArgsMock(NavigationMode mode, bool isCancelable)
        {
            _mode = mode;
            _isCancelable = isCancelable;
        }

        #endregion

        #region Overrides of NavigatingCancelEventArgsBase

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode
        {
            get { return _mode; }
        }

        public override bool IsCancelable
        {
            get { return _isCancelable; }
        }

        #endregion
    }
}
