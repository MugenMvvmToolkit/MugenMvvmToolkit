#region Copyright

// ****************************************************************************
// <copyright file="OperationCallbackManager.cs">
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
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure.Callbacks
{
    /// <summary>
    ///     Represents the callback manager.
    /// </summary>
    public class OperationCallbackManager : IOperationCallbackManager
    {
        #region Nested types

        //NOTE we cannot use default dictionary, because MONO cannot deserialize it correctly.
        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
        internal sealed class CallbackDictionary : LightDictionaryBase<string, List<object>>
        {
            #region Constructors

            /// <summary>
            ///     Initializes a new instance of the <see cref="CallbackDictionary" /> class.
            /// </summary>
            public CallbackDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,List<object>>

            /// <summary>
            ///     Determines whether the specified objects are equal.
            /// </summary>
            protected override bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.Ordinal);
            }

            /// <summary>
            ///     Returns a hash code for the specified object.
            /// </summary>
            protected override int GetHashCode(string key)
            {
                return key.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string CallbacksMember = "~~#callbacks";

        private static readonly DataConstant<CallbackDictionary> CallbackConstant;

        private readonly object _locker;

        #endregion

        #region Constructors

        static OperationCallbackManager()
        {
            CallbackConstant = DataConstant.Create(() => CallbackConstant, true);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationCallbackManager" /> class.
        /// </summary>
        public OperationCallbackManager()
        {
            _locker = new object();
        }

        #endregion

        #region Properties

        /// <summary>
        ///    Gets or sets the value, if <c>true</c> manager will serialize callback before store it, use this to debug your callbacks.
        /// </summary>
        public static bool AlwaysSerializeCallback { get; set; }

        #endregion

        #region Implementation of IOperationCallbackManager

        /// <summary>
        ///     Registers the specified operation callback.
        /// </summary>
        public void Register(OperationType operation, object source, IOperationCallback callback, IDataContext context)
        {
            Should.NotBeNull(operation, "operation");
            Should.NotBeNull(source, "source");
            Should.NotBeNull(callback, "callback");
            if (context == null)
                context = DataContext.Empty;
            RegisterInternal(operation, source, callback, context);
            Tracer.Info("Callback '{0}' was registered, source: '{1}'", operation, source);
        }

        /// <summary>
        ///     Sets the result of operation.
        /// </summary>
        public void SetResult(object source, IOperationResult result)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(result, "result");
            SetResultInternal(source, result);
            Tracer.Info("Callback '{0}' was invoked, source: '{1}'", result.Operation, source);
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Registers the specified operation callback.
        /// </summary>
        protected virtual void RegisterInternal(OperationType operation, [NotNull] object target,
            [NotNull] IOperationCallback callback, [NotNull] IDataContext context)
        {
            CallbackDictionary callbacks;
            var viewModel = target as IViewModel;
            if (viewModel != null && callback.IsSerializable)
            {
                lock (_locker)
                {
                    if (!viewModel.Settings.State.TryGetData(CallbackConstant, out callbacks))
                    {
                        callbacks = new CallbackDictionary();
                        viewModel.Settings.State.Add(CallbackConstant, callbacks);
                    }
                }
            }
            else
                callbacks = ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(target, CallbacksMember, (o, o1) => new CallbackDictionary(), null);
            RegisterInternal(callbacks, operation.Id, callback);
        }

        /// <summary>
        ///     Sets the result of operation.
        /// </summary>
        protected virtual void SetResultInternal([NotNull] object target, [NotNull] IOperationResult result)
        {
            string id = result.Operation.Id;
            IEnumerable<object> callbacks = null;
            var viewModel = target as IViewModel;
            if (viewModel != null)
            {
                lock (_locker)
                {
                    CallbackDictionary data;
                    if (viewModel.Settings.State.TryGetData(CallbackConstant, out data))
                    {
                        lock (data)
                        {
                            List<object> list;
                            if (data.TryGetValue(id, out list))
                            {
                                callbacks = list;
                                data.Remove(id);
                            }
                            if (data.Count == 0)
                                viewModel.Settings.State.Remove(CallbackConstant);
                        }
                    }
                }
            }
            var attachedValue = ServiceProvider
                .AttachedValueProvider
                .GetValue<Dictionary<string, List<object>>>(target, CallbacksMember, false);
            if (attachedValue != null)
            {
                lock (attachedValue)
                {
                    List<object> list;
                    if (attachedValue.TryGetValue(id, out list))
                    {
                        callbacks = callbacks == null ? list : list.Concat(callbacks);
                        attachedValue.Remove(id);
                    }
                }
            }
            if (callbacks == null)
            {
                Tracer.Warn("The callbacks for operation '{0}' was not found, source: '{1}'", id, target);
                return;
            }
            foreach (IOperationCallback callback in callbacks.OfType<IOperationCallback>())
                callback.Invoke(result);
        }

        private static void RegisterInternal(CallbackDictionary callbacks, string id, IOperationCallback callback)
        {
            //Only for debug callback
            if (AlwaysSerializeCallback && callback.IsSerializable)
            {
                ISerializer serializer;
                if (ServiceProvider.IocContainer.TryGet(out serializer))
                {
                    var stream = serializer.Serialize(callback);
                    stream.Position = 0;
                    callback = (IOperationCallback)serializer.Deserialize(stream);
                }
            }

            lock (callbacks)
            {
                List<object> list;
                if (!callbacks.TryGetValue(id, out list))
                {
                    list = new List<object>();
                    callbacks[id] = list;
                }
                list.Add(callback);
            }
        }

        #endregion
    }
}