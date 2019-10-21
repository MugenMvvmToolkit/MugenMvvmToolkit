using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class ExpressionCompilerComponent : AttachableComponentBase<IExpressionCompiler>, IExpressionCompilerComponent, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>//todo check fast compiler
    {
        #region Fields

        protected readonly IReflectionDelegateProvider? DelegateProvider;
        protected readonly IMetadataContextProvider? MetadataContextProvider;

        protected IExpressionBuilder[] Builders;

        #endregion

        #region Constructors

        public ExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null, IReflectionDelegateProvider? delegateProvider = null)
        {
            MetadataContextProvider = metadataContextProvider;
            DelegateProvider = delegateProvider;
            Builders = Default.EmptyArray<IExpressionBuilder>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>.OnAdded(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentAdded(collection, component, metadata);
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>.OnRemoved(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            OnComponentRemoved(collection, component, metadata);
        }

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return Compile(expression, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            Builders = owner.Components.GetItems().OfType<IExpressionBuilder>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            Builders = Default.EmptyArray<IExpressionBuilder>();
        }

        protected virtual ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new CompiledExpression(this, expression, metadata);
        }

        protected virtual void OnComponentAdded(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref Builders, collection, component);
        }

        protected virtual void OnComponentRemoved(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref Builders, component);
        }

        #endregion

        #region Nested types

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            ParameterExpression MetadataParameter { get; }

            IBindingParameterInfo? TryGetLambdaParameter();

            void SetLambdaParameter(IBindingParameterInfo parameter);

            void ClearLambdaParameter(IBindingParameterInfo parameter);

            ParameterExpression? TryGetParameterExpression(IParameterExpression expression);

            void SetParameterExpression(IParameterExpression expression, ParameterExpression value);

            void ClearParameterExpression(IParameterExpression expression);

            Expression Build(IExpressionNode expression);
        }

        public interface IExpressionBuilder : IComponent<IExpressionCompiler>
        {
            Expression? TryBuild(IContext context, IExpressionNode expression);
        }

        private sealed class CompiledExpression : LightDictionary<object, DelegateInvoker>, ICompiledExpression, IContext, IExpressionVisitor
        {
            #region Fields

            private readonly ExpressionCompilerComponent _compiler;
            private readonly IExpressionNode _expression;
            private readonly IReadOnlyMetadataContext? _inputMetadata;
            private readonly ParameterDictionary _parametersDict;
            private readonly object?[] _values;

            private List<IBindingParameterInfo>? _lambdaParameters;
            private IMetadataContext? _metadata;

            private static readonly ParameterExpression MetadataParameterExpression = Expression.Parameter(typeof(IReadOnlyMetadataContext), "metadata");

            #endregion

            #region Constructors

            public CompiledExpression(ExpressionCompilerComponent compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
            {
                _compiler = compiler;
                _inputMetadata = metadata;
                _parametersDict = new ParameterDictionary();
                _expression = expression.Accept(this);
                _values = new object[_parametersDict.Count + 1];
            }

            #endregion

            #region Properties

            public bool HasMetadata => _metadata != null || _inputMetadata != null && _inputMetadata.Count != 0;

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        Interlocked.CompareExchange(ref _metadata, _inputMetadata.ToNonReadonly(this, _compiler.MetadataContextProvider), null);
                    return _metadata!;
                }
            }

            public ParameterExpression MetadataParameter => MetadataParameterExpression;

            bool IExpressionVisitor.IsPostOrder => false;

            #endregion

            #region Implementation of interfaces

            public object? Invoke(ExpressionValue[] values, IReadOnlyMetadataContext? metadata)
            {
                Should.NotBeNull(values, nameof(values));
                if (!TryGetValue(values, out var invoker))
                {
                    invoker = CompileExpression(values);
                    var types = new Type[values.Length];
                    for (var i = 0; i < values.Length; i++)
                        types[i] = values[i].Type;
                    this[types] = invoker;
                }

                for (var i = 0; i < values.Length; i++)
                    _values[i] = values[i].Value;
                _values[_values.Length - 1] = metadata;
                try
                {
                    return invoker.Invoke(_values);
                }
                finally
                {
                    Array.Clear(_values, 0, _values.Length);
                }
            }

            public IBindingParameterInfo? TryGetLambdaParameter()
            {
                return _lambdaParameters?.FirstOrDefault();
            }

            public void SetLambdaParameter(IBindingParameterInfo parameter)
            {
                Should.NotBeNull(parameter, nameof(parameter));
                if (_lambdaParameters == null)
                    _lambdaParameters = new List<IBindingParameterInfo>(2);
                _lambdaParameters.Insert(0, parameter);
            }

            public void ClearLambdaParameter(IBindingParameterInfo parameter)
            {
                Should.NotBeNull(parameter, nameof(parameter));
                _lambdaParameters?.Remove(parameter);
            }

            public ParameterExpression? TryGetParameterExpression(IParameterExpression expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _parametersDict.TryGetValue(expression, out var value);
                return value;
            }

            public void SetParameterExpression(IParameterExpression expression, ParameterExpression value)
            {
                Should.NotBeNull(expression, nameof(expression));
                Should.NotBeNull(value, nameof(value));
                _parametersDict[expression] = value;
            }

            public void ClearParameterExpression(IParameterExpression expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _parametersDict.Remove(expression);
            }

            public Expression Build(IExpressionNode expression)
            {
                var components = _compiler.Builders;
                foreach (var component in components)
                {
                    var compile = component.TryBuild(this, expression);
                    if (compile != null)
                        return compile;
                }

                BindingExceptionManager.ThrowCannotCompileExpression(expression);
                return null!;
            }

            IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
            {
                if (node.NodeType == ExpressionNodeType.BindingMember)
                    _parametersDict[(IParameterExpression)node] = null;
                return node;
            }

            #endregion

            #region Methods

            private DelegateInvoker CompileExpression(ExpressionValue[] values)
            {
                try
                {
                    var parameters = new ParameterExpression[_values.Length];
                    foreach (var value in _parametersDict)
                    {
                        var parameterExpression = value.Key;
                        if (parameterExpression.NodeType == ExpressionNodeType.BindingMember)
                        {
                            var parameter = Expression.Parameter(values[parameterExpression.Index].Type, parameterExpression.Name);
                            parameters[parameterExpression.Index] = parameter;
                            _parametersDict[parameterExpression] = parameter;
                        }
                    }

                    parameters[parameters.Length - 1] = MetadataParameterExpression;

                    var expression = Build(_expression);
                    var compile = Expression.Lambda(expression, parameters).Compile();//todo review single compile for array
                    var methodInfo = compile.GetType().GetMethodUnified(nameof(Action.Invoke), MemberFlags.Public | MemberFlags.Instance);
                    if (methodInfo == null)
                        return new DelegateInvoker(compile, null);

                    return new DelegateInvoker(compile, methodInfo.GetMethodInvoker(_compiler.DelegateProvider));
                }
                finally
                {
                    _lambdaParameters?.Clear();
                    if (_metadata != null)
                    {
                        _metadata.Clear();
                        if (_inputMetadata != null && _inputMetadata.Count != 0)
                            _metadata.Merge(_inputMetadata);
                    }
                }
            }

            protected override bool Equals(object x, object y)
            {
                var typesX = x as Type[];
                var typesY = y as Type[];
                if (typesX == null && typesY == null)
                {
                    var valuesX = (ExpressionValue[])x;
                    var valuesY = (ExpressionValue[])y;
                    if (valuesX.Length != valuesY.Length)
                        return false;
                    for (var i = 0; i < valuesX.Length; i++)
                    {
                        if (!valuesX[i].Type.EqualsEx(valuesY[i].Type))
                            return false;
                    }

                    return true;
                }

                if (typesX == null)
                    return Equals(typesY!, (ExpressionValue[])x);
                if (typesY == null)
                    return Equals(typesX!, (ExpressionValue[])y);

                if (typesX.Length != typesY.Length)
                    return false;
                for (var i = 0; i < typesX.Length; i++)
                {
                    if (!typesX[i].EqualsEx(typesY[i]))
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(object key)
            {
                unchecked
                {
                    var hash = 0;
                    if (key is ExpressionValue[] values)
                    {
                        for (var index = 0; index < values.Length; index++)
                            hash = hash * 397 ^ values[index].Type.GetHashCode();
                    }
                    else
                    {
                        var types = (Type[])key;
                        for (var index = 0; index < types.Length; index++)
                            hash = hash * 397 ^ types[index].GetHashCode();
                    }

                    return hash;
                }
            }

            private static bool Equals(Type[] types, ExpressionValue[] values)
            {
                if (values.Length != types.Length)
                    return false;
                for (var i = 0; i < values.Length; i++)
                {
                    if (!values[i].Type.EqualsEx(types[i]))
                        return false;
                }

                return true;
            }

            #endregion
        }

        private sealed class ParameterDictionary : LightDictionary<IParameterExpression, ParameterExpression?>
        {
            #region Constructors

            public ParameterDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IParameterExpression key)
            {
                if (key.NodeType == ExpressionNodeType.BindingMember)
                {
                    unchecked
                    {
                        return key.Index * 397 ^ key.Name.GetHashCode();
                    }
                }

                return RuntimeHelpers.GetHashCode(key);
            }

            protected override bool Equals(IParameterExpression x, IParameterExpression y)
            {
                if (x.NodeType == ExpressionNodeType.BindingMember && y.NodeType == ExpressionNodeType.BindingMember)
                    return x.Index == y.Index && x.Name == y.Name;
                return ReferenceEquals(x, y);
            }

            #endregion
        }

        [StructLayout(LayoutKind.Auto)]
        private readonly struct DelegateInvoker
        {
            #region Fields

            private readonly Delegate _delegate;
            private readonly Func<object?, object?[], object?>? _invoker;

            #endregion

            #region Constructors

            public DelegateInvoker(Delegate @delegate, Func<object?, object?[], object?>? invoker)
            {
                _delegate = @delegate;
                _invoker = invoker;
            }

            #endregion

            #region Methods

            public object? Invoke(object?[] args)
            {
                if (_invoker == null)
                    return _delegate.DynamicInvoke(args);
                return _invoker.Invoke(_delegate, args);
            }

            #endregion
        }

        #endregion
    }
}