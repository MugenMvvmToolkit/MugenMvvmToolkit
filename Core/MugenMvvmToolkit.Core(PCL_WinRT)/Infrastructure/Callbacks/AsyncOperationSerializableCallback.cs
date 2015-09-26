#region Copyright

// ****************************************************************************
// <copyright file="AsyncOperationSerializableCallback.cs">
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

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
    internal class AsyncOperationSerializableCallback : ISerializableCallback
    {
        #region Fields

        [DataMember(EmitDefaultValue = false)]
        internal List<object> Callbacks;

        [DataMember(EmitDefaultValue = false)]
        internal bool IsFunc;

        [DataMember(EmitDefaultValue = false)]
        internal ISerializableCallback MainCallback;

        [DataMember(EmitDefaultValue = false)]
        internal string InputType;

        [DataMember(EmitDefaultValue = false)]
        internal string OutputType;

        #endregion

        #region Constructors

        public AsyncOperationSerializableCallback(ISerializableCallback mainCallback, string inputType,
            string outputType, bool isFunc, List<object> callbacks)
        {
            MainCallback = mainCallback;
            InputType = inputType;
            OutputType = outputType;
            Callbacks = callbacks;
            IsFunc = isFunc;
        }

        #endregion

        #region Implementation of ISerializableCallback

        public object Invoke(IOperationResult result)
        {
            var outType = Type.GetType(OutputType, true);
            try
            {
                if (InputType != null)
                {
                    var inType = Type.GetType(InputType, true);
                    result = OperationResult.Convert(inType, result);
                }
                if (MainCallback != null)
                {
                    object obj = MainCallback.Invoke(result);
                    if (IsFunc)
                        result = OperationResult.CreateResult(outType, result.Operation, result.Source, obj,
                            result.OperationContext);
                    else
                        result = OperationResult.Convert(outType, result);
                }
            }
            catch (Exception e)
            {
                Tracer.Error(e.Flatten(false));
                if (result.Exception != null)
                    e = new AggregateException(result.Exception, e);
                else if (e is OperationCanceledException)
                    e = null;
                if (e == null)
                    result = OperationResult.CreateCancelResult(outType, result.Operation, result.Source,
                        result.OperationContext);
                else
                    result = OperationResult.CreateErrorResult(outType, result.Operation, result.Source,
                        e, result.OperationContext);
            }
            finally
            {
                if (Callbacks != null)
                {
                    for (int i = 0; i < Callbacks.Count; i++)
                        ((ISerializableCallback)Callbacks[i]).Invoke(result);
                }
            }
            return null;
        }

        #endregion
    }
}
