#region Copyright

// ****************************************************************************
// <copyright file="IocParameter.cs">
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
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces;

namespace MugenMvvmToolkit.Models.IoC
{
    [Serializable, DataContract(Namespace = ApplicationSettings.DataContractNamespace)]
    public class IocParameter : IIocParameter
    {
        #region Fields

        #endregion

        #region Constructors

        public IocParameter(string name, object value, IocParameterType parameterType)
        {
            Name = name;
            Value = value;
            ParameterType = parameterType;
        }

        #endregion

        #region Properties

        [DataMember(Name = "pt")]
        public IocParameterType ParameterType { get; internal set; }

        [DataMember(Name = "n")]
        public string Name { get; internal set; }

        [DataMember(Name = "v")]
        public object Value { get; internal set; }

        #endregion
    }
}
