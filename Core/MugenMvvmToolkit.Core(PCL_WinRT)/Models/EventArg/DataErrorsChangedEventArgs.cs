#region Copyright

// ****************************************************************************
// <copyright file="DataErrorsChangedEventArgs.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

#if NONOTIFYDATAERROR
using System;
using MugenMvvmToolkit.Interfaces.Validation;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class DataErrorsChangedEventArgs : EventArgs
    {
        #region Fields

        private readonly string _propertyName;

        #endregion

        #region Constructors

        public DataErrorsChangedEventArgs(string propertyName)
        {
            _propertyName = propertyName;
        }

        #endregion

        #region Properties

        public string PropertyName
        {
            get { return _propertyName; }
        }

        #endregion
    }
}
#endif
