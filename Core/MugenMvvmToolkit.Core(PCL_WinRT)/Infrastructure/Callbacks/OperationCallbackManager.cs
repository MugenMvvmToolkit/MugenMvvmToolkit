#region Copyright

// ****************************************************************************
// <copyright file="OperationCallbackManager.cs">
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
        private readonly ISerializer _serializer;

        #endregion

        #region Constructors

        static OperationCallbackManager()
        {
            CallbackConstant = DataConstant.Create<CallbackDictionary>(typeof(OperationCallbackManager), nameof(CallbackConstant), true);
        }

        public OperationCallbackManager(ISerializer serializer)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            _serializer = serializer;
        }

        #endregion

        #region Properties

        public static bool AlwaysSerializeCallback { get; set; }

        #endregion

        #region Implementation of IOperationCallbackManager

        public void Register(OperationType operation, object target, IOperationCallback callback, IDataContext context)
        {
            Should.NotBeNull(operation, nameof(operation));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(callback, nameof(callback));
            if (context == null)
                context = DataContext.Empty;
            RegisterInternal(operation, target, callback, context);
            if (Tracer.TraceInformation)
                Tracer.Info("Callback '{0}' was registered, target: '{1}'", operation, target);
        }

        public void SetResult(IOperationResult result)
        {
            Should.NotBeNull(result, nameof(result));
            var invoked = SetResultInternal(result);
            if (Tracer.TraceInformation)
            {
                if (invoked)
                    Tracer.Info("The callback '{0}' was invoked, source: '{1}'", result.Operation, result.Source);
                else
                    Tracer.Info("The callback '{0}' was not found, source: '{1}'", result.Operation.Id, result.Source);
            }
        }

        public void SetResult(object target, Func<OperationType, object, IOperationResult> getResult)
        {
            Should.NotBeNull(target, nameof(target));
            if (getResult == null)
                getResult = (type, o) => OperationResult.CreateCancelResult<bool?>(type, o);
            SetResultInternal(target, getResult);
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
                if (!viewModel.Settings.State.TryGetData(CallbackConstant, out callbacks))
                {
                    callbacks = new CallbackDictionary();
                    viewModel.Settings.State.AddOrUpdate(CallbackConstant, callbacks);
                }
            }
            else
                callbacks = ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(target, CallbacksMember, (o, o1) => new CallbackDictionary(), null);
            RegisterInternal(callbacks, operation.Id, callback);
        }

        protected virtual void SetResultInternal([NotNull] object target, [NotNull] Func<OperationType, object, IOperationResult> getResult)
        {
            CallbackDictionary data;
            List<string> l1 = null;
            List<string> l2 = null;
            var viewModel = target as IViewModel;
            if (viewModel != null && viewModel.Settings.State.TryGetData(CallbackConstant, out data))
                l1 = InvokeCallbacks(data, viewModel, getResult, vm => vm.Settings.State.Remove(CallbackConstant));

            if (ServiceProvider.AttachedValueProvider.TryGetValue(target, CallbacksMember, out data))
                l2 = InvokeCallbacks(data, target, getResult, o => ServiceProvider.AttachedValueProvider.Clear(o, CallbacksMember));

            if (Tracer.TraceInformation && l1 != null || l2 != null)
            {
                var set = new HashSet<string>();
                if (l1 != null)
                    set.AddRange(l1);
                if (l2 != null)
                    set.AddRange(l2);
                foreach (var s in set)
                    Tracer.Info("The callback '{0}' was invoked, source: '{1}'", s, target);
            }
        }

        protected virtual bool SetResultInternal([NotNull] IOperationResult result)
        {
            var target = result.Source;
            List<IOperationCallback> callbacks = null;
            CallbackDictionary data;

            var viewModel = target as IViewModel;
            if (viewModel != null && viewModel.Settings.State.TryGetData(CallbackConstant, out data))
                InitializeCallbacks(data, result.Operation, ref callbacks, viewModel, vm => vm.Settings.State.Remove(CallbackConstant));

            if (ServiceProvider.AttachedValueProvider.TryGetValue(target, CallbacksMember, out data))
                InitializeCallbacks(data, result.Operation, ref callbacks, target, o => ServiceProvider.AttachedValueProvider.Clear(o, CallbacksMember));

            if (callbacks == null)
                return false;
            foreach (var callback in callbacks)
                callback.Invoke(result);
            return true;
        }

        private static void InitializeCallbacks<T>(CallbackDictionary dictionary, OperationType type, ref List<IOperationCallback> list, T target, Action<T> clearDictAction)
        {
            if (dictionary == null)
                return;
            List<object> value;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(type.Id, out value))
                    dictionary.Remove(type.Id);
                if (dictionary.Count == 0)
                    clearDictAction(target);
            }
            if (value == null)
                return;
            if (list == null)
                list = new List<IOperationCallback>();
            lock (value)
                list.AddRange(value.Cast<IOperationCallback>());
        }

        private static List<string> InvokeCallbacks<T>(CallbackDictionary dictionary, T target, Func<OperationType, object, IOperationResult> getResult, Action<T> clearDictAction)
        {
            List<string> invoked = null;
            KeyValuePair<string, List<object>>[] keyValuePairs;
            lock (dictionary)
                keyValuePairs = dictionary.ToArray();
            foreach (var pair in keyValuePairs)
            {
                var op = new OperationType(pair.Key);
                var result = getResult(op, target);
                if (result == null)
                    continue;

                List<IOperationCallback> list = null;
                InitializeCallbacks(dictionary, op, ref list, target, clearDictAction);
                if (list == null)
                    continue;
                foreach (var callback in list)
                    callback.Invoke(result);
                if (invoked == null)
                    invoked = new List<string>();
                invoked.Add(pair.Key);
            }
            return invoked;
        }

        private void RegisterInternal(CallbackDictionary callbacks, string id, IOperationCallback callback)
        {
            //Only for debug
            if (AlwaysSerializeCallback && callback.IsSerializable)
            {
                var stream = _serializer.Serialize(callback);
                stream.Position = 0;
                callback = (IOperationCallback)_serializer.Deserialize(stream);
            }

            List<object> list;
            lock (callbacks)
            {
                if (!callbacks.TryGetValue(id, out list))
                {
                    list = new List<object>();
                    callbacks[id] = list;
                }
            }
            lock (list)
                list.Add(callback);
        }

        #endregion
    }
}
