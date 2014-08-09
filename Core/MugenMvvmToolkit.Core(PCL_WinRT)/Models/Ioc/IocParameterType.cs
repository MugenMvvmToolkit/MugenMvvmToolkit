#region Copyright
// ****************************************************************************
// <copyright file="IocParameterType.cs">
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
namespace MugenMvvmToolkit.Models.IoC
{
    /// <summary>
    ///     Represents the parameter types.
    /// </summary>
    public class IocParameterType : StringConstantBase<IocParameterType>
    {
        #region Fields

        /// <summary>
        ///     Constructor parameter.
        /// </summary>
        public static readonly IocParameterType Constructor = new IocParameterType("Constructor");

        /// <summary>
        ///     Property parameter.
        /// </summary>
        public static readonly IocParameterType Property = new IocParameterType("Property");

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="IocParameterType" /> class.
        /// </summary>
        public IocParameterType(string id)
            : base(id)
        {
        }

        #endregion
    }
}