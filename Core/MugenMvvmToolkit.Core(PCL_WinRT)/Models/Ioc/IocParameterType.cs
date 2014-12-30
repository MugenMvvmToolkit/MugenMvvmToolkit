#region Copyright

// ****************************************************************************
// <copyright file="IocParameterType.cs">
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
        public static readonly IocParameterType Constructor;

        /// <summary>
        ///     Property parameter.
        /// </summary>
        public static readonly IocParameterType Property;

        #endregion

        #region Constructors

        static IocParameterType()
        {
            Constructor = new IocParameterType("Constructor");
            Property = new IocParameterType("Property");
        }

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