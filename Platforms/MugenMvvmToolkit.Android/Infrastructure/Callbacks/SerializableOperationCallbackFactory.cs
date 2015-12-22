#region Copyright

// ****************************************************************************
// <copyright file="SerializableOperationCallbackFactory.cs">
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

#if ANDROID || TOUCH || WPF || WINFORMS
extern alias mscore;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

#if ANDROID
namespace MugenMvvmToolkit.Android.Infrastructure.Callbacks
#elif TOUCH
namespace MugenMvvmToolkit.iOS.Infrastructure.Callbacks
#elif WINDOWSCOMMON
namespace MugenMvvmToolkit.WinRT.Infrastructure.Callbacks
#elif WINDOWS_PHONE
namespace MugenMvvmToolkit.WinPhone.Infrastructure.Callbacks
#elif XAMARIN_FORMS
namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Callbacks
#elif WPF
namespace MugenMvvmToolkit.WPF.Infrastructure.Callbacks
#elif WINFORMS
namespace MugenMvvmToolkit.WinForms.Infrastructure.Callbacks
#elif SILVERLIGHT
namespace MugenMvvmToolkit.Silverlight.Infrastructure.Callbacks
#endif
{
    //NOTE do you want to see some magic? :)
    public class SerializableOperationCallbackFactory : IOperationCallbackFactory
    {
        #region Nested types

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true, Name = "socffs")]
#if ANDROID || TOUCH || WPF || WINFORMS
        [mscore::System.Serializable]
#endif
        internal sealed class FieldSnapshot
        {
            #region Fields

            private const int AsyncOperationField = 0;
            private const int SerializableField = 1;
            private const int AwaiterField = 2;
            private const int NonSerializableField = 3;
            private const int ViewModelField = 4;
            private const int AnonymousClass = 5;
            private const int NavigationOperationField = 6;
            private const int BuilderField = 7;

            #endregion

            #region Properties

            [DataMember(Name = "n", EmitDefaultValue = false)]
            public string Name { get; set; }

            [DataMember(Name = "t", EmitDefaultValue = false)]
            public string TypeName { get; set; }

            [DataMember(Name = "s", EmitDefaultValue = false)]
            public object State { get; set; }

            [DataMember(Name = "sn", EmitDefaultValue = false)]
            public List<FieldSnapshot> Snapshots { get; set; }

            [DataMember(Name = "f", EmitDefaultValue = false)]
            public int FieldType { get; set; }

            [DataMember(Name = "ist", EmitDefaultValue = false)]
            public bool IsType { get; set; }

            #endregion

            #region Methods

            public bool Restore(Type targetType, object target, Dictionary<Type, object> items, ICollection<IViewModel> viewModels, string awaiterResultType, IOperationResult result)
            {
                var field = targetType.GetFieldEx(Name, MemberFlags.NonPublic | MemberFlags.Public | MemberFlags.Instance);
                if (field == null)
                {
                    TraceError(null, targetType);
                    return false;
                }
                switch (FieldType)
                {
                    case BuilderField:
                        var type = Type.GetType(TypeName, true);
                        var createMethod = type.GetMethodEx("Create", MemberFlags.NonPublic | MemberFlags.Public | MemberFlags.Static);
                        var startMethod = type.GetMethodEx("Start", MemberFlags.NonPublic | MemberFlags.Public | MemberFlags.Instance);
                        if (createMethod == null || startMethod == null || !startMethod.IsGenericMethodDefinition)
                        {
                            ((IAsyncStateMachine)target).MoveNext();
                            return true;
                        }
                        var builder = createMethod.Invoke(null, Empty.Array<object>());
                        SetValue(field, target, builder);
                        startMethod.MakeGenericMethod(typeof(IAsyncStateMachine))
                                   .Invoke(builder, new[] { target });
                        break;
                    case AwaiterField:
                        var awaiterType = typeof(SerializableAwaiter<>).MakeGenericType(Type.GetType(awaiterResultType, true));
                        var instance = Activator.CreateInstance(awaiterType, result);
                        SetValue(field, target, instance);
                        break;
                    case AsyncOperationField:
                        var opType = typeof(AsyncOperation<>).MakeGenericType(Type.GetType(awaiterResultType, true));
                        var opInstance = (IAsyncOperation)Activator.CreateInstance(opType);
                        AsyncOperation<object>.TrySetResult(opInstance, result);
                        SetValue(field, target, opInstance);
                        break;
                    case AnonymousClass:
                        var anonType = Type.GetType(TypeName, true);
                        object anonClass;
                        if (!items.TryGetValue(anonType, out anonClass))
                        {
                            anonClass = ServiceProvider.GetOrCreate(anonType);
                            foreach (var snapshot in Snapshots)
                                snapshot.Restore(anonType, anonClass, items, viewModels, awaiterResultType, result);
                            items[anonType] = anonClass;
                        }
                        SetValue(field, target, anonClass);
                        break;
                    case NavigationOperationField:
                        var operation = new NavigationOperation();
                        operation.SetResult(OperationResult.Convert<bool>(result));
                        SetValue(field, target, operation);
                        break;
                    case NonSerializableField:
                        object service;
                        if (State == null)
                        {
                            var serviceType = Type.GetType(TypeName, true);
                            if (!items.TryGetValue(serviceType, out service))
                            {
                                if (field.Name.Contains("CachedAnonymousMethodDelegate"))
                                    service = field.GetValueEx<object>(target);
                                else if (!ServiceProvider.TryGet(serviceType, out service))
                                {
                                    service = field.GetValueEx<object>(target);
                                    TraceError(field, targetType);
                                }
                                items[serviceType] = service;
                            }
                        }
                        else
                        {
                            var stateManager = ServiceProvider.OperationCallbackStateManager;
                            service = stateManager == null ? State : stateManager.RestoreValue(State, field, items, viewModels, result, DataContext.Empty);
                        }
                        SetValue(field, target, service);
                        break;
                    case SerializableField:
                        SetValue(field, target, IsType ? Type.GetType((string)State, false) : State);
                        break;
                    case ViewModelField:
                        var viewModel = RestoreViewModel(viewModels, items, result);
                        if (viewModel == null)
                        {
                            TraceError(field, targetType);
                            return false;
                        }
                        SetValue(field, target, viewModel);
                        break;
                }
                return true;
            }

            public static FieldSnapshot Create(FieldInfo field, object target, IAsyncOperation asyncOperation, ISerializer serializer)
            {
                var isStateMachine = target is IAsyncStateMachine;
                if (isStateMachine)
                {
                    if (field.Name == "$Builder" || field.Name == "<>t__builder")
                        return new FieldSnapshot
                        {
                            FieldType = BuilderField,
                            Name = field.Name,
                            TypeName = field.FieldType.AssemblyQualifiedName
                        };
                }

                var value = field.GetValueEx<object>(target);
                if (value == null || value is IAsyncStateMachine)
                    return null;

                if (isStateMachine && value is IAsyncOperationAwaiter)
                    return new FieldSnapshot { Name = field.Name, FieldType = AwaiterField };

                //NavigationOperation
                if (value is INavigationOperation)
                {
                    return new FieldSnapshot
                    {
                        Name = field.Name,
                        FieldType = NavigationOperationField
                    };
                }

                //field is type.
                if (typeof(Type).IsAssignableFrom(field.FieldType))
                    return new FieldSnapshot
                    {
                        State = ((Type)value).AssemblyQualifiedName,
                        FieldType = SerializableField,
                        Name = field.Name,
                        IsType = true
                    };

                var stateManager = ServiceProvider.OperationCallbackStateManager;
                if (stateManager != null)
                {
                    var valueState = stateManager.SaveValue(value, field, asyncOperation, DataContext.Empty);
                    if (valueState != null)
                        return new FieldSnapshot
                        {
                            Name = field.Name,
                            State = valueState,
                            FieldType = NonSerializableField
                        };
                }

                var viewModel = value as IViewModel;
                if (viewModel != null)
                {
                    return new FieldSnapshot
                    {
                        Name = field.Name,
                        FieldType = ViewModelField,
                        TypeName = value.GetType().AssemblyQualifiedName,
                        State = viewModel.GetViewModelId()
                    };
                }

                if (serializer.IsSerializable(field.FieldType) || value is string)
                    return new FieldSnapshot
                    {
                        State = value,
                        FieldType = SerializableField,
                        Name = field.Name
                    };
                //Anonymous class
                if (field.FieldType.IsAnonymousClass())
                {
                    var type = value.GetType();
                    var snapshots = new List<FieldSnapshot>();
                    foreach (var anonymousField in type.GetFieldsEx(MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public))
                    {
                        var snapshot = Create(anonymousField, value, asyncOperation, serializer);
                        if (snapshot != null)
                            snapshots.Add(snapshot);
                    }
                    return new FieldSnapshot
                    {
                        FieldType = AnonymousClass,
                        Name = field.Name,
                        Snapshots = snapshots,
                        TypeName = type.AssemblyQualifiedName
                    };
                }
                if (asyncOperation != null && Equals(value, asyncOperation))
                    return new FieldSnapshot
                    {
                        Name = field.Name,
                        FieldType = AsyncOperationField
                    };
                return new FieldSnapshot
                {
                    Name = field.Name,
                    TypeName = value.GetType().AssemblyQualifiedName,
                    FieldType = NonSerializableField
                };
            }

            private IViewModel RestoreViewModel(ICollection<IViewModel> viewModels, Dictionary<Type, object> items, IOperationResult result)
            {
                Guid id = Guid.Empty;
                var vmType = Type.GetType(TypeName, true);
                if (State != null)
                    Guid.TryParse(State.ToString(), out id);
                var vm = ServiceProvider.ViewModelProvider.TryGetViewModelById(id);
                if (vm != null)
                    return vm;

                var stateManager = ServiceProvider.OperationCallbackStateManager;
                if (stateManager != null)
                {
                    vm = stateManager.RestoreViewModelValue(vmType, id, items, viewModels, result, DataContext.Empty);
                    if (vm != null)
                        return vm;
                }

                foreach (var viewModel in viewModels)
                {
                    if (viewModel.GetViewModelId() == id)
                        return viewModel;
                    if (viewModel.GetType() == vmType)
                        vm = viewModel;
                }
                return vm;
            }

            private void TraceError(FieldInfo field, Type stateMachineType)
            {
                string fieldSt = field == null ? Name : field.ToString();
                Tracer.Error("The field '{0}' cannot be restored on type '{1}'", fieldSt, stateMachineType);
            }

            private static void SetValue(FieldInfo field, object target, object value)
            {
                value = ReflectionExtensions.Convert(value, field.FieldType);
                field.SetValueEx(target, value);
            }

            #endregion
        }

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
#if ANDROID || TOUCH || WPF || WINFORMS
        [mscore::System.Serializable]
#endif
        internal sealed class AwaiterSerializableCallback : ISerializableCallback
        {
            #region Constructors

            //Only for serialization
            internal AwaiterSerializableCallback() { }

            public AwaiterSerializableCallback(Action continuation, IAsyncStateMachine stateMachine, string awaiterResultType, bool isUiThread,
                IAsyncOperation asyncOperation, ISerializer serializer)
            {
                IsUiThread = isUiThread;
                AwaiterResultType = awaiterResultType;
                Initialize(continuation, stateMachine, asyncOperation, serializer);
            }

            #endregion

            #region Properties

            [DataMember(Name = "art", EmitDefaultValue = false)]
            public string AwaiterResultType { get; set; }

            [DataMember(Name = "smt", EmitDefaultValue = false)]
            public string StateMachineType { get; set; }

            [DataMember(Name = "fs", EmitDefaultValue = false)]
            public List<FieldSnapshot> FieldSnapshots { get; set; }

            [DataMember(Name = "iut", EmitDefaultValue = false)]
            public bool IsUiThread { get; set; }

            #endregion

            #region Methods

            private void Initialize(Action continuation, IAsyncStateMachine stateMachine, IAsyncOperation asyncOperation, ISerializer serializer)
            {
                const MemberFlags flags = MemberFlags.NonPublic | MemberFlags.Public | MemberFlags.Instance;
                if (stateMachine == null)
                {
                    stateMachine = continuation.Target as IAsyncStateMachine;
                    if (stateMachine == null)
                    {
                        var fieldInfo = continuation.Target
                            .GetType()
                            .GetFieldEx("m_continuation", flags);
                        if (fieldInfo != null)
                            continuation = fieldInfo.GetValueEx<Action>(continuation.Target);

                        fieldInfo = continuation.Target
                            .GetType()
                            .GetFieldEx("m_stateMachine", flags);
                        if (fieldInfo == null)
                        {
                            TraceError(continuation.Target);
                            return;
                        }
                        stateMachine = fieldInfo.GetValueEx<IAsyncStateMachine>(continuation.Target);
                        if (stateMachine == null)
                        {
                            TraceError(continuation.Target);
                            return;
                        }
                    }
                }
                var type = stateMachine.GetType();
                StateMachineType = type.AssemblyQualifiedName;
                FieldSnapshots = new List<FieldSnapshot>();

                foreach (var field in type.GetFieldsEx(flags))
                {
                    var snapshot = FieldSnapshot.Create(field, stateMachine, asyncOperation, serializer);
                    if (snapshot != null)
                        FieldSnapshots.Add(snapshot);
                }
            }

            private void InvokeInternal(IOperationResult result)
            {
                if (StateMachineType == null || FieldSnapshots == null)
                {
                    Tracer.Error("The await callback cannot be executed empty serialization state property " +
                                 (StateMachineType == null ? "StateMachineType" : "FieldSnapshots"));
                    return;
                }
                var type = Type.GetType(StateMachineType, true);
                IAsyncStateMachine stateMachine = null;
#if WINDOWSCOMMON || XAMARIN_FORMS
                if (type.GetTypeInfo().IsValueType)
#else
                if (type.IsValueType)
#endif
                {
                    try
                    {
#if WINDOWSCOMMON
                        stateMachine = (IAsyncStateMachine)Activator.CreateInstance(type);
#else
                        stateMachine = (IAsyncStateMachine)GetDefault(type);
#endif
                    }
                    catch
                    {
                        ;
                    }
                }
                else
                {
                    try
                    {
                        var constructor = type.GetConstructor(Empty.Array<Type>());
                        if (constructor != null)
                            stateMachine = (IAsyncStateMachine)constructor.InvokeEx();
                    }
                    catch
                    {
                        ;
                    }
                }

                if (stateMachine == null)
                {
                    Exception e = null;
                    try
                    {
#if WINDOWSCOMMON
                        if (type.GetTypeInfo().IsValueType)
                            stateMachine = (IAsyncStateMachine)GetDefault(type);
                        else
                            stateMachine = (IAsyncStateMachine)Activator.CreateInstance(type);
#else
                        stateMachine = (IAsyncStateMachine)Activator.CreateInstance(type);
#endif
                    }
                    catch (Exception ex)
                    {
                        e = ex;
                    }
                    if (e != null)
                    {
                        Tracer.Error("The await callback cannot be executed missing constructor, state machine " + type +
                                     " " + e.Flatten(true));
                        return;
                    }
                }

                var viewModels = CollectViewModels(result);
                var items = new Dictionary<Type, object>();
                if (result.Source != null)
                    items[result.Source.GetType()] = result.Source;
                //we need to sort fields, to restore builder as last operation.
                FieldSnapshots.Sort((x1, x2) => x1.FieldType.CompareTo(x2.FieldType));
                for (int index = 0; index < FieldSnapshots.Count; index++)
                {
                    var fieldSnapshot = FieldSnapshots[index];
                    if (!fieldSnapshot.Restore(type, stateMachine, items, viewModels, AwaiterResultType, result))
                    {
                        object fieldInfo = (object)type.GetFieldEx(fieldSnapshot.Name,
                            MemberFlags.NonPublic | MemberFlags.Public | MemberFlags.Instance) ?? fieldSnapshot.Name;
                        Tracer.Error("The await callback cannot be executed, field ({0}) cannot be restored source {1}", fieldInfo, result.Source);
                        break;
                    }
                }
            }

            private static void TraceError(object target)
            {
                Tracer.Error("The serializable awaiter cannot get IAsyncStateMachine from target {0}", target);
            }

            #endregion

            #region Implementation of ISerializableCallback

            public object Invoke(IOperationResult result)
            {
                if (IsUiThread)
                    ServiceProvider.ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, result, (callback, operationResult) => callback.InvokeInternal(operationResult));
                else
                    InvokeInternal(result);
                return null;
            }

            #endregion
        }

        private sealed class AwaiterContinuation : IActionContinuation
        {
            #region Fields

            private readonly Action _continuation;
            private readonly IHasStateMachine _hasStateMachine;
            private readonly Type _resultType;
            private readonly IAsyncOperation _asyncOperation;
            private readonly ISerializer _serializer;
            private ISerializableCallback _serializableCallback;
            private readonly SynchronizationContext _context;
            private readonly bool _isUiThread;

            #endregion

            #region Constructors

            public AwaiterContinuation(Action continuation, IHasStateMachine hasStateMachine, Type resultType, bool continueOnCapturedContext,
                IAsyncOperation asyncOperation, ISerializer serializer)
            {
                Should.NotBeNull(continuation, nameof(continuation));
                _continuation = continuation;
                _hasStateMachine = hasStateMachine;
                _resultType = resultType;
                _asyncOperation = asyncOperation;
                _serializer = serializer;
                if (continueOnCapturedContext)
                {
                    _context = SynchronizationContext.Current;
                    _isUiThread = ServiceProvider.ThreadManager.IsUiThread;
                }
            }

            #endregion

            #region Implementation of IContinuation

            public ISerializableCallback ToSerializableCallback()
            {
                if (_serializableCallback == null)
                    _serializableCallback = new AwaiterSerializableCallback(_continuation, _hasStateMachine.StateMachine, _resultType.AssemblyQualifiedName,
                        _isUiThread, _asyncOperation, _serializer);
                return _serializableCallback;
            }

            public void Invoke(IOperationResult result)
            {
                if (_context == null || ReferenceEquals(SynchronizationContext.Current, _context))
                    _continuation();
                else
                    _context.Post(state => ((Action)state).Invoke(), _continuation);
            }

            #endregion
        }

        private interface IHasStateMachine
        {
            IAsyncStateMachine StateMachine { get; }
        }

        private sealed class SerializableAwaiter<TResult> : IAsyncOperationAwaiter, IAsyncOperationAwaiter<TResult>, IAsyncStateMachineAware, IHasStateMachine
        {
            #region Fields

            private readonly IOperationResult _result;
            private readonly IAsyncOperation _operation;
            private readonly bool _continueOnCapturedContext;
            private readonly ISerializer _serializer;
            private IAsyncStateMachine _stateMachine;

            #endregion

            #region Constructors

            [UsedImplicitly]
            public SerializableAwaiter(IOperationResult result)
            {
                if (result != null)
                    _result = OperationResult.Convert<TResult>(result);
            }

            public SerializableAwaiter(IAsyncOperation operation, bool continueOnCapturedContext, ISerializer serializer)
            {
                _operation = operation;
                _continueOnCapturedContext = continueOnCapturedContext;
                _serializer = serializer;
            }

            #endregion

            #region Implementation of IAsyncOperationAwaiter

            public void OnCompleted(Action continuation)
            {
                _operation.ContinueWith(new AwaiterContinuation(continuation, this, typeof(TResult), _continueOnCapturedContext, _operation, _serializer));
            }

            public bool IsCompleted => _result != null || _operation.IsCompleted;

            TResult IAsyncOperationAwaiter<TResult>.GetResult()
            {
                IOperationResult result = _result ?? _operation.Result;
                return (TResult)result.Result;
            }

            void IAsyncOperationAwaiter.GetResult()
            {
                IOperationResult result = _result ?? _operation.Result;
                // ReSharper disable once UnusedVariable
                var o = result.Result;
            }

            void IAsyncStateMachineAware.SetStateMachine(IAsyncStateMachine stateMachine)
            {
                _stateMachine = stateMachine;
            }

            #endregion

            #region Implementation of IHasStateMachine

            public IAsyncStateMachine StateMachine => _stateMachine;

            #endregion
        }

        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true)]
#if ANDROID || TOUCH || WPF || WINFORMS
        [mscore::System.Serializable]
#endif
        internal sealed class DelegateSerializableCallback : ISerializableCallback
        {
            #region Fields

            [DataMember(Name = "tt", EmitDefaultValue = false)]
            internal string TargetType;

            [DataMember(Name = "fps", EmitDefaultValue = false)]
            internal bool FirstParameterSource;

            [DataMember(Name = "is", EmitDefaultValue = false)]
            internal bool IsStatic;

            [DataMember(Name = "t", EmitDefaultValue = false)]
            internal object Target;

            [DataMember(Name = "mn", EmitDefaultValue = false)]
            internal string MethodName;

            [DataMember(Name = "s", EmitDefaultValue = false)]
            internal List<FieldSnapshot> Snapshots;

            #endregion

            #region Constructors

            //Only for serialization
            internal DelegateSerializableCallback() { }

            public DelegateSerializableCallback(string targetType, string methodName, bool firstParameterSource, bool isStatic, object target, List<FieldSnapshot> snapshots)
            {
                Should.NotBeNull(targetType, nameof(targetType));
                Should.NotBeNull(methodName, nameof(methodName));
                Snapshots = snapshots;
                TargetType = targetType;
                MethodName = methodName;
                FirstParameterSource = firstParameterSource;
                IsStatic = isStatic;
                Target = target;
            }

            #endregion

            #region Implementation of ISerializableCallback

            public object Invoke(IOperationResult result)
            {
                var invokeInternal = InvokeInternal(result);
                if (Tracer.TraceInformation)
                    Tracer.Info("The restored callback was invoked, target type '{0}', method '{1}'", TargetType, MethodName);
                return invokeInternal;
            }

            #endregion

            #region Methods

            private object InvokeInternal(IOperationResult result)
            {
                var type = Type.GetType(TargetType, true);
                var flags = MemberFlags.Public | MemberFlags.NonPublic |
                                                    (IsStatic ? MemberFlags.Static : MemberFlags.Instance);
                var method = type.GetMethodsEx(flags).First(FilterMethod);
                var viewModels = CollectViewModels(result);
                var items = new Dictionary<Type, object>();
                if (result.Source != null)
                    items[result.Source.GetType()] = result.Source;
                object[] args;
                if (FirstParameterSource)
                {
                    var parameter = method.GetParameters()[0];
                    object firstParam;
                    if (!items.TryGetValue(parameter.ParameterType, out firstParam))
                    {
                        var viewModel = viewModels.FirstOrDefault(model => model.GetType() == parameter.ParameterType);
                        firstParam = viewModel ?? result.Source;
                    }
                    args = new[] { firstParam, result };
                }
                else
                    args = new object[] { result };
                if (IsStatic)
                    return method.InvokeEx(null, args);

                object target = Target;
                if (target == null)
                {
                    if (!items.TryGetValue(type, out target))
                    {
                        target = ServiceProvider.GetOrCreate(type);
                        items[type] = target;
                    }
                }
                if (Snapshots != null)
                {
                    foreach (var fieldSnapshot in Snapshots)
                        fieldSnapshot.Restore(type, target, items, viewModels, null, result);
                }
                return method.InvokeEx(target, args);
            }

            private bool FilterMethod(MethodInfo method)
            {
                if (method.Name != MethodName)
                    return false;
                var parameters = method.GetParameters();
                if (FirstParameterSource)
                {
                    if (parameters.Length == 2 && typeof(IOperationResult).IsAssignableFrom(parameters[1].ParameterType))
                        return true;
                }
                else
                {
                    if (parameters.Length == 1 && typeof(IOperationResult).IsAssignableFrom(parameters[0].ParameterType))
                        return true;
                }
                return false;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly MethodInfo GetDefaultGenericMethod;
        private readonly ISerializer _serializer;

        #endregion

        #region Constructors

        static SerializableOperationCallbackFactory()
        {
            GetDefaultGenericMethod = typeof(SerializableOperationCallbackFactory)
                .GetMethodEx(nameof(GetDefaultGeneric), MemberFlags.Public | MemberFlags.NonPublic | MemberFlags.Static);
        }

        public SerializableOperationCallbackFactory(ISerializer serializer)
        {
            Should.NotBeNull(serializer, nameof(serializer));
            _serializer = serializer;
        }

        #endregion

        #region Implementation of IOperationCallbackFactory

        public IAsyncOperationAwaiter CreateAwaiter(IAsyncOperation operation, IDataContext context)
        {
            return CreateAwaiterInternal<object>(operation, context);
        }

        public IAsyncOperationAwaiter<TResult> CreateAwaiter<TResult>(IAsyncOperation<TResult> operation, IDataContext context)
        {
            return CreateAwaiterInternal<TResult>(operation, context);
        }

        public ISerializableCallback CreateSerializableCallback(Delegate @delegate)
        {
            Should.NotBeNull(@delegate, nameof(@delegate));
            var method = @delegate.GetMethodInfo();
            bool firstParameterSource;
            if (!CheckMethodParameters(method, out firstParameterSource))
            {
                Tracer.Warn("The method '{0}' cannot be serialized, invalid parameters.", method);
                return null;
            }
            if (method.IsStatic)
                return new DelegateSerializableCallback(method.DeclaringType.AssemblyQualifiedName, method.Name,
                    firstParameterSource, true, null, null);

            var target = @delegate.Target;
            var targetType = target.GetType();
            if (targetType.IsAnonymousClass())
            {
                var snapshots = new List<FieldSnapshot>();
                foreach (var anonymousField in targetType.GetFieldsEx(MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public))
                {
                    var snapshot = FieldSnapshot.Create(anonymousField, target, null, _serializer);
                    if (snapshot != null)
                        snapshots.Add(snapshot);
                }
                return new DelegateSerializableCallback(targetType.AssemblyQualifiedName, method.Name, firstParameterSource,
                    false, null, snapshots);
            }
            return new DelegateSerializableCallback(targetType.AssemblyQualifiedName, method.Name, firstParameterSource,
                false, _serializer.IsSerializable(targetType) ? target : null, null);
        }

        #endregion

        #region Methods

        private SerializableAwaiter<TResult> CreateAwaiterInternal<TResult>(IAsyncOperation operation, IDataContext context)
        {
            Should.NotBeNull(operation, nameof(operation));
            if (context == null)
                context = DataContext.Empty;
            bool continueOnCapturedContext;
            if (!context.TryGetData(OpeartionCallbackConstants.ContinueOnCapturedContext, out continueOnCapturedContext))
                continueOnCapturedContext = true;
            return new SerializableAwaiter<TResult>(operation, continueOnCapturedContext, _serializer);
        }


        private static object GetDefault(Type t)
        {
            return GetDefaultGenericMethod.MakeGenericMethod(t).InvokeEx(null, null);
        }

        private static ICollection<IViewModel> CollectViewModels(IOperationResult result)
        {
            var viewModels = new HashSet<IViewModel>(ReferenceEqualityComparer.Instance);
            var context = result.OperationContext as INavigationContext;
            if (context != null)
            {
                CollectViewModels(viewModels, context.ViewModelTo);
                CollectViewModels(viewModels, context.ViewModelFrom);
            }
            var viewModel = result.Source as IViewModel;
            if (viewModel != null)
                CollectViewModels(viewModels, viewModel);
            return viewModels;
        }

        private static void CollectViewModels(ICollection<IViewModel> viewModels, IViewModel viewModel)
        {
            while (true)
            {
                if (viewModel == null || viewModels.Contains(viewModel))
                    return;
                var parentViewModel = viewModel.GetParentViewModel();
                if (parentViewModel != null)
                    CollectViewModels(viewModels, parentViewModel);

                viewModels.Add(viewModel);
                var wrapperViewModel = viewModel as IWrapperViewModel;
                if (wrapperViewModel == null)
                    break;
                viewModel = wrapperViewModel.ViewModel;
            }
        }

        private static bool CheckMethodParameters(MethodInfo method, out bool firstParameterSource)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                firstParameterSource = false;
                return true;
            }
            if (parameters.Length == 1 && typeof(IOperationResult).IsAssignableFrom(parameters[0].ParameterType))
            {
                firstParameterSource = false;
                return true;
            }
            if (parameters.Length == 2 && typeof(IOperationResult).IsAssignableFrom(parameters[1].ParameterType))
            {
                firstParameterSource = true;
                return true;
            }
            firstParameterSource = false;
            return false;
        }

        internal static T GetDefaultGeneric<T>()
        {
            return default(T);
        }

        #endregion
    }
}
