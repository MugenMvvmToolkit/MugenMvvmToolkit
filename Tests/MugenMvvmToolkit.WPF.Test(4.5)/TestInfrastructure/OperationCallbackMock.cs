#region Copyright

// ****************************************************************************
// <copyright file="OperationCallbackMock.cs">
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
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class OperationCallbackMock : IOperationCallback
    {
        #region Properties

        public Action<IOperationResult> Invoke { get; set; }

        #endregion

        #region Implementation of IOperationCallback

        public bool IsSerializable { get; set; }

        void IOperationCallback.Invoke(IOperationResult result)
        {
            Invoke(result);
        }

        #endregion
    }
}
