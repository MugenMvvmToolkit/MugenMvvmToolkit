#region Copyright
// ****************************************************************************
// <copyright file="NavigationType.cs">
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
namespace MugenMvvmToolkit.Models
{
    public class NavigationType : StringConstantBase<NavigationType>
    {
        #region Fields

        public static readonly NavigationType Undefined;

        public static readonly NavigationType Tab;
        
        public static readonly NavigationType Window;

        public static readonly NavigationType Page;

        #endregion

        #region Constructors

        static NavigationType()
        {
            Tab = new NavigationType("Tab");
            Window = new NavigationType("Window");
            Page = new NavigationType("Page");
            Undefined = new NavigationType("Undefined");
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationType" /> class.
        /// </summary>
        public NavigationType(string id)
            : base(id)
        {
        }

        #endregion
    }
}