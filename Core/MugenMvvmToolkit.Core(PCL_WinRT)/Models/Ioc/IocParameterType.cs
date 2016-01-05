#region Copyright

// ****************************************************************************
// <copyright file="IocParameterType.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public class IocParameterType : StringConstantBase<IocParameterType>
    {
        #region Fields

        public static readonly IocParameterType Constructor;

        public static readonly IocParameterType Property;

        #endregion

        #region Constructors

        static IocParameterType()
        {
            Constructor = new IocParameterType("Constructor");
            Property = new IocParameterType("Property");
        }

        public IocParameterType(string id)
            : base(id)
        {
        }

        #endregion
    }
}
