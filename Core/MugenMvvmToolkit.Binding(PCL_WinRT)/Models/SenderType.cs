#region Copyright
// ****************************************************************************
// <copyright file="SenderType.cs">
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
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Models
{
    /// <summary>
    ///     Represents the type of sender.
    /// </summary>
    public class SenderType : StringConstantBase<SenderType>
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="SenderType" /> class.
        /// </summary>
        public SenderType(string id)
            : base(id)
        {
        }

        #endregion
    }
}