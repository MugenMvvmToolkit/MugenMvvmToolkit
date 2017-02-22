using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.UWP.Models.EventArg
{
    public class RemoveNavigationEventArgs : NavigationEventArgsBase
    {
        #region Fields

        private readonly IDataContext _context;

        #endregion

        #region Constructors

        public RemoveNavigationEventArgs(IDataContext context)
        {
            _context = context;
        }

        #endregion

        #region Properties

        public override object Content => null;

        public override NavigationMode NavigationMode => NavigationMode.Remove;

        public override IDataContext Context => _context;

        #endregion
    }
}