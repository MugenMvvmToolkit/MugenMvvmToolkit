#region Copyright

// ****************************************************************************
// <copyright file="ForegroundNavigationMessage.cs">
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

using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models.Messages
{
    public class ForegroundNavigationMessage
    {
        #region Constructors

        public ForegroundNavigationMessage(IDataContext context = null)
        {
            Context = context;
        }

        #endregion

        #region Properties

        public IDataContext Context { get; }

        #endregion
    }
}