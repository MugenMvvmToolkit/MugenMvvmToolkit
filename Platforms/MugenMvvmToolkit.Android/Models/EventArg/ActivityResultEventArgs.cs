using Android.App;
using Android.Content;

namespace MugenMvvmToolkit.Android.Models.EventArg
{
    public class ActivityResultEventArgs
    {
        #region Fields

        private readonly Intent _data;
        private readonly int _requestCode;
        private readonly Result _resultCode;

        #endregion

        #region Constructors

        public ActivityResultEventArgs(int requestCode, Result resultCode, Intent data)
        {
            _requestCode = requestCode;
            _resultCode = resultCode;
            _data = data;
        }

        #endregion

        #region Properties

        public Intent Data => _data;

        public int RequestCode => _requestCode;

        public Result ResultCode => _resultCode;

        #endregion

    }
}