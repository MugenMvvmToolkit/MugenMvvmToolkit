#region Copyright

// ****************************************************************************
// <copyright file="ApplicationStartingMessage.cs">
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

namespace MugenMvvmToolkit.Xamarin.Forms.Models.Messages
{
    public class ApplicationStartingMessage
    {
        #region Fields

        public static readonly ApplicationStartingMessage Instance = new ApplicationStartingMessage();

        #endregion

        #region Constructors

        private ApplicationStartingMessage()
        {
        }

        #endregion
    }
}