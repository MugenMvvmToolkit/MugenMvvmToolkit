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
    public class OperationCallbackManager : IOperationCallbackManager
    {
        #region Nested types

        //NOTE we cannot use default dictionary, because MONO cannot deserialize it correctly.
        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
        internal sealed class CallbackDictionary : LightDictionaryBase<string, List<object>>
        {
            #region Constructors

            public CallbackDictionary()
                : base(true)
            {
            }

            #endregion

            #region Overrides of LightDictionaryBase<string,List<object>>

            protected override bool Equals(string x, string y)
            {
                return x.Equals(y, StringComparison.Ordinal);
            }

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
        private readonly ISerializer _serializer;

        #endregion

        #region Constructors

        static OperationCallbackManager()
        {
            CallbackConstant = DataConstant.Create(() => CallbackConstant, true);
        }

        public OperationCallbackManager(ISerializer serializer)
        {
            Should.NotBeNull(serializer, "serializer");
            _serializer = serializer;
            _locker = new object();
        }

        #endregion

        #region Properties

        public static bool AlwaysSerializeCallback { get; set; }

        #endregion

        #region Implementation of IOperationCallbackManager

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

        public void SetResult(object source, IOperationResult result)
        {
            Should.NotBeNull(source, "source");
            Should.NotBeNull(result, "result");
            SetResultInternal(source, result);
            Tracer.Info("Callback '{0}' was invoked, source: '{1}'", result.Operation, source);
        }

        #endregion

        #region Methods

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
                Tracer.Info("The callbacks for operation '{0}' was not found, source: '{1}'", id, target);
                return;
            }
            foreach (IOperationCallback callback in callbacks.OfType<IOperationCallback>())
                callback.Invoke(result);
        }

        private void RegisterInternal(CallbackDictionary callbacks, string id, IOperationCallback callback)
        {
            //Only for debug callback
            if (AlwaysSerializeCallback && callback.IsSerializable)
            {
                var stream = _serializer.Serialize(callback);
                stream.Position = 0;
                callback = (IOperationCallback)_serializer.Deserialize(stream);
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
