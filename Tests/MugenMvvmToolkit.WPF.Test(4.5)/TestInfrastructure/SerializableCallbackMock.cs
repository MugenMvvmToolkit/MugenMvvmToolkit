#region Copyright

// ****************************************************************************
// <copyright file="SerializableCallbackMock.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    [DataContract]
    public class SerializableCallbackMock : ISerializableCallback
    {
        #region Properties

        public Func<IOperationResult, object> Invoke { get; set; }

        #endregion

        #region Implementation of ISerializableCallback

        object ISerializableCallback.Invoke(IOperationResult result)
        {
            if (Invoke == null)
                return null;
            return Invoke(result);
        }

        #endregion
    }
}
