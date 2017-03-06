#region Copyright

// ****************************************************************************
// <copyright file="SerializerDataContainer.cs">
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

namespace MugenMvvmToolkit.Models
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true, Name = "sdc")]
    [Serializable]
    public sealed class SerializerDataContainer
    {
        #region Properties

        [DataMember(Name = "d")]
        public object Data { get; set; }

        #endregion
    }
}