using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.UWP.Models.EventArg
{
    public class RemoveNavigatingCancelEventArgs : NavigatingCancelEventArgsBase
    {
        #region Fields

        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public RemoveNavigatingCancelEventArgs(IDataContext context)
        {
            _context = context;
        }

        #endregion

        #region Properties

        public override bool Cancel { get; set; }

        public override NavigationMode NavigationMode => NavigationMode.Remove;

        public override bool IsCancelable => true;

        public override IDataContext Context => _context;

        #endregion
    }
}