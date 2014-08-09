#region Copyright
// ****************************************************************************
// <copyright file="AsyncOperationCallback.cs">
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
using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvmToolkit.Interfaces.Callbacks;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
    internal sealed class AsyncOperationCallback : IOperationCallback
    {
        #region Fields

        [IgnoreDataMember, XmlIgnore]
        private ISerializableCallback _callback;

        [NonSerialized, IgnoreDataMember, XmlIgnore]
        private readonly IAsyncOperationInternal _asyncOperation;

        #endregion

        #region Constructors

        public AsyncOperationCallback(IAsyncOperationInternal asyncOperation)
        {
            Should.NotBeNull(asyncOperation, "asyncOperation");
            _asyncOperation = asyncOperation;
        }

        #endregion

        #region Properties

        [DataMember]
        internal ISerializableCallback Callback
        {
            get
            {
                InitializeCallback();
                return _callback;
            }
            set { _callback = value; }
        }

        #endregion

        #region Methods

        private void InitializeCallback()
        {
            if (_callback == null && _asyncOperation != null)
                _callback = _asyncOperation.ToSerializableCallback();
        }

        #endregion

        #region Implementation of IOperationCallback

        /// <summary>
        ///     Gets a value indicating whether the <see cref="IOperationCallback" /> is serializable.
        /// </summary>
        public bool IsSerializable
        {
            get { return _callback != null || _asyncOperation != null; }
        }

        /// <summary>
        ///     Invokes the callback using the specified operation result.
        /// </summary>
        public void Invoke(IOperationResult result)
        {
            Should.NotBeNull(result, "result");
            if (_asyncOperation == null)
            {
                if (_callback != null)
                    _callback.Invoke(result);
            }
            else
                _asyncOperation.SetResult(result, true);
        }

        #endregion
    }
}