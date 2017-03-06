#region Copyright

// ****************************************************************************
// <copyright file="OperationCallbackManagerMock.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Test.TestInfrastructure
{
    public class OperationCallbackManagerMock : IOperationCallbackManager
    {
        #region Properties

        public Action<OperationType, object, IOperationCallback, IDataContext> Register { get; set; }

        public Action<object, IOperationResult> SetResult { get; set; }

        #endregion

        #region Implementation of IOperationCallbackManager

        void IOperationCallbackManager.Register(OperationType operation, object source, IOperationCallback callback,
            IDataContext context)
        {
            if (Register == null)
                Tracer.Warn("OperationCallbackManagerMock: Register == null");
            else
                Register(operation, source, callback, context);
        }

        void IOperationCallbackManager.SetResult(IOperationResult result)
        {
            if (SetResult == null)
                Tracer.Warn("OperationCallbackManagerMock: SetResult == null");
            else
                SetResult(result.Source, result);
        }

        void IOperationCallbackManager.SetResult(object target, Func<OperationType, object, IOperationResult> getResult)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
