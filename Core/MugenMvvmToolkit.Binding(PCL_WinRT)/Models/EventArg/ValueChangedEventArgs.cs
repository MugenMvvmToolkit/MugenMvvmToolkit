#region Copyright
// ****************************************************************************
// <copyright file="ValueChangedEventArgs.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class ValueChangedEventArgs : EventArgs
    {
        #region Fields

        public static readonly ValueChangedEventArgs TrueEventArgs = new ValueChangedEventArgs(true);

        public static readonly ValueChangedEventArgs FalseEventArgs = new ValueChangedEventArgs(false);

        private readonly bool _lastMemberChanged;

        #endregion

        #region Constructors

        protected ValueChangedEventArgs(bool lastMemberChanged)
        {
            _lastMemberChanged = lastMemberChanged;
        }

        #endregion

        #region Properties

        public bool LastMemberChanged
        {
            get { return _lastMemberChanged; }
        }

        #endregion
    }
}