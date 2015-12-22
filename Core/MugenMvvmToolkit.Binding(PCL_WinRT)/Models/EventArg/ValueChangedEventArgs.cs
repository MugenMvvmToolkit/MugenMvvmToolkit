#region Copyright

// ****************************************************************************
// <copyright file="ValueChangedEventArgs.cs">
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

using System;

namespace MugenMvvmToolkit.Binding.Models.EventArg
{
    public class ValueChangedEventArgs : EventArgs
    {
        #region Fields

        public static readonly ValueChangedEventArgs TrueEventArgs;
        public static readonly ValueChangedEventArgs FalseEventArgs;

        private readonly bool _lastMemberChanged;

        #endregion

        #region Constructors

        static ValueChangedEventArgs()
        {
            TrueEventArgs = new ValueChangedEventArgs(true);
            FalseEventArgs = new ValueChangedEventArgs(false);
        }

        protected ValueChangedEventArgs(bool lastMemberChanged)
        {
            _lastMemberChanged = lastMemberChanged;
        }

        #endregion

        #region Properties

        public bool LastMemberChanged => _lastMemberChanged;

        #endregion
    }
}
