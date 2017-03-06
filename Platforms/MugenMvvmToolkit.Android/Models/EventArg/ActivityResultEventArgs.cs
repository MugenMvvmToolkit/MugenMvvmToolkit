#region Copyright

// ****************************************************************************
// <copyright file="ActivityResultEventArgs.cs">
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