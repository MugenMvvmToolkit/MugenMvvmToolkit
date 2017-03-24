#region Copyright

// ****************************************************************************
// <copyright file="ViewModelRestoringEventArgs.cs">
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models.EventArg
{
    public class ViewModelRestoringEventArgs : EventArgs
    {
        #region Fields

        private IDataContext _state;

        #endregion

        #region Properties

        [NotNull]
        public IDataContext State
        {
            get { return _state; }
            set
            {
                Should.PropertyNotBeNull(value);
                _state = value;
            }
        }

        public IDataContext Context { get; set; }

        #endregion
    }
}