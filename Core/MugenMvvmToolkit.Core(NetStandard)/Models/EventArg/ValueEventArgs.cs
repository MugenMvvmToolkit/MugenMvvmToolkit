#region Copyright

// ****************************************************************************
// <copyright file="ValueEventArgs.cs">
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

using System;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ValueEventArgs<TValue> : EventArgs
    {
        #region Constructors

        public ValueEventArgs(TValue value)
        {
            Value = value;
        }

        #endregion

        #region Properties

        public TValue Value { get; }

        #endregion
    }
}
