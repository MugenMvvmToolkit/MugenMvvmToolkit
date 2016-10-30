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

using System;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Attributes;

namespace MugenMvvmToolkit.Models.IoC
{
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
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

        //Only for serialization
        [Preserve]
        internal IocParameterType() { }

        public IocParameterType(string id)
            : base(id)
        {
        }

        #endregion
    }
}
