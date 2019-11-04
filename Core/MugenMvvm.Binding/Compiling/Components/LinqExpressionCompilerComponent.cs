using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Compiling.Components
{
    public sealed class LinqExpressionCompilerComponent : AttachableComponentBase<IExpressionCompiler>, IExpressionCompilerComponent, IHasPriority,
        IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>
    {
        #region Fields

        private readonly IMetadataContextProvider? _metadataContextProvider;
        private ILinqExpressionBuilderComponent[] _builders;

        #endregion

        #region Constructors

        public LinqExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _builders = Default.EmptyArray<ILinqExpressionBuilderComponent>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>.OnAdded(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnAdded(ref _builders, collection, component);
        }

        void IComponentCollectionChangedListener<IComponent<IExpressionCompiler>>.OnRemoved(IComponentCollection<IComponent<IExpressionCompiler>> collection,
            IComponent<IExpressionCompiler> component, IReadOnlyMetadataContext? metadata)
        {
            MugenExtensions.ComponentTrackerOnRemoved(ref _builders, component);
        }

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return new CompiledExpression(this, expression, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            _builders = owner.Components.GetItems().OfType<ILinqExpressionBuilderComponent>().ToArray();
            owner.Components.Components.Add(this);
        }

        protected override void OnDetachedInternal(IExpressionCompiler owner, IReadOnlyMetadataContext? metadata)
        {
            owner.Components.Components.Remove(this);
            _builders = Default.EmptyArray<ILinqExpressionBuilderComponent>();
        }

        #endregion

        #region Nested types

        private sealed class CompiledExpression : LightDictionary<object, Func<object?[], object?>>, ICompiledExpression, ILinqExpressionBuilderContext, IExpressionVisitor
        {
            #region Fields

            private readonly LinqExpressionCompilerComponent _compiler;
            private readonly IExpressionNode _expression;
            private readonly IReadOnlyMetadataContext? _inputMetadata;
            private readonly ParameterDictionary _parametersDict;
            private readonly object?[] _values;

            private List<IBindingParameterInfo>? _lambdaParameters;
            private IMetadataContext? _metadata;

            private static readonly ParameterExpression ArrayParameter = Expression.Parameter(typeof(object[]), "args");
            private static readonly ParameterExpression[] ArrayParameterArray = { ArrayParameter };
            private static readonly Expression[] ArrayIndexesCache = GenerateArrayIndexes(25);

            #endregion

            #region Constructors

            public CompiledExpression(LinqExpressionCompilerComponent compiler, IExpressionNode expression, IReadOnlyMetadataContext? metadata)
            {
                _compiler = compiler;
                _inputMetadata = metadata;
                _parametersDict = new ParameterDictionary();
                _expression = expression.Accept(this);
                _values = new object[_parametersDict.Count + 1];
                MetadataParameter = GetIndexExpression(_parametersDict.Count).ConvertIfNeed(typeof(IReadOnlyMetadataContext), false);
            }

            #endregion

            #region Properties

            public bool HasMetadata => !(_metadata ?? _inputMetadata).IsNullOrEmpty();

            public IMetadataContext Metadata
            {
                get
                {
                    if (_metadata == null)
                        _compiler._metadataContextProvider.LazyInitialize(ref _metadata, this, _inputMetadata);
                    return _metadata!;
                }
            }

            public Expression MetadataParameter { get; }

            bool IExpressionVisitor.IsPostOrder => false;

            #endregion

            #region Implementation of interfaces

            public object? Invoke(ItemOrList<ExpressionValue, ExpressionValue[]> values, IReadOnlyMetadataContext? metadata)
            {
                var list = values.List;
                var key = list ?? values.Item.Type ?? (object)Default.EmptyArray<Type>();
                if (!TryGetValue(key, out var invoker))
                {
                    invoker = CompileExpression(values);
                    if (list == null)
                        this[key] = invoker;
                    else
                    {
                        var types = new Type[list.Length];
                        for (var i = 0; i < list.Length; i++)
                            types[i] = list[i].Type;
                        this[types] = invoker;
                    }
                }

                if (list == null)
                    _values[0] = values.Item.Value;
                else
                {
                    for (var i = 0; i < list.Length; i++)
                        _values[i] = list[i].Value;
                }

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

            IExpressionNode IExpressionVisitor.Visit(IExpressionNode node)
            {
                if (node.NodeType == ExpressionNodeType.BindingMember)
                {
                    var parameterExpression = (IParameterExpressionNode)node;
                    if (parameterExpression.Index < 0)
                        BindingExceptionManager.ThrowCannotCompileExpression(parameterExpression);
                    _parametersDict[parameterExpression] = null;
                }

                return node;
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

            public Expression? TryGetExpression(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _parametersDict.TryGetValue(expression, out var value);
                return value;
            }

            public void SetExpression(IExpressionNode expression, Expression value)
            {
                Should.NotBeNull(expression, nameof(expression));
                Should.NotBeNull(value, nameof(value));
                _parametersDict[expression] = value;
            }

            public void ClearExpression(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                _parametersDict.Remove(expression);
            }

            public Expression Build(IExpressionNode expression)
            {
                Should.NotBeNull(expression, nameof(expression));
                var components = _compiler._builders;
                Expression? exp;
                foreach (var component in components)
                {
                    exp = component.TryBuild(this, expression);
                    if (exp != null)
                        return exp;
                }

                exp = TryGetExpression(expression);
                if (exp != null)
                    return exp;

                BindingExceptionManager.ThrowCannotCompileExpression(expression);
                return null!;
            }

            #endregion

            #region Methods

            private Func<object?[], object?> CompileExpression(ItemOrList<ExpressionValue, ExpressionValue[]> values)
            {
                try
                {
                    var expressionValues = values.List;
                    foreach (var value in _parametersDict)
                    {
                        var ex = value.Key;
                        if (ex.NodeType == ExpressionNodeType.BindingMember && ex is IParameterExpressionNode parameterExpression)
                        {
                            var index = GetIndexExpression(parameterExpression.Index);
                            if (expressionValues == null)
                            {
                                if (parameterExpression.Index != 0)
                                    ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(values));
                                _parametersDict[parameterExpression] = index.ConvertIfNeed(values.Item.Type, false);
                            }
                            else
                                _parametersDict[parameterExpression] = index.ConvertIfNeed(expressionValues[parameterExpression.Index].Type, false);
                        }
                    }

                    var expression = Build(_expression).ConvertIfNeed(typeof(object), false);
                    var lambda = Expression.Lambda<Func<object?[], object?>>(expression, ArrayParameterArray);
                    return lambda.CompileEx();
                }
                finally
                {
                    _lambdaParameters?.Clear();
                    if (_metadata != null)
                    {
                        _metadata.Clear();
                        if (!_inputMetadata.IsNullOrEmpty())
                            _metadata.Merge(_inputMetadata!);
                    }
                }
            }

            protected override bool Equals(object x, object y)
            {
                var typeX = x as Type;
                var typeY = y as Type;
                if (typeX != null || typeY != null)
                {
                    if (typeX == null || typeY == null)
                        return false;
                    return typeX == typeY;
                }

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
                        if (valuesX[i].Type != valuesY[i].Type)
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
                    if (typesX[i] != typesY[i])
                        return false;
                }

                return true;
            }

            protected override int GetHashCode(object key)
            {
                if (key is Type type)
                    return type.GetHashCode();

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
                    if (values[i].Type != types[i])
                        return false;
                }

                return true;
            }

            private static Expression GetIndexExpression(int index)
            {
                if (index >= 0 && index < ArrayIndexesCache.Length)
                    return ArrayIndexesCache[index];
                return Expression.ArrayIndex(ArrayParameter, MugenExtensions.GetConstantExpression(index));
            }

            private static Expression[] GenerateArrayIndexes(int length)
            {
                var expressions = new Expression[length];
                for (var i = 0; i < length; i++)
                    expressions[i] = Expression.ArrayIndex(ArrayParameter, MugenExtensions.GetConstantExpression(i));
                return expressions;
            }

            #endregion
        }

        private sealed class ParameterDictionary : LightDictionary<IExpressionNode, Expression?>
        {
            #region Constructors

            public ParameterDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IExpressionNode key)
            {
                if (key.NodeType == ExpressionNodeType.BindingMember && key is IParameterExpressionNode parameter)
                {
                    unchecked
                    {
                        return parameter.Index * 397 ^ parameter.Name.GetHashCode();
                    }
                }

                return RuntimeHelpers.GetHashCode(key);
            }

            protected override bool Equals(IExpressionNode x, IExpressionNode y)
            {
                if (x.NodeType == ExpressionNodeType.BindingMember && y.NodeType == ExpressionNodeType.BindingMember)
                {
                    var xP = (IParameterExpressionNode)x;
                    var yP = (IParameterExpressionNode)y;
                    return xP.Index == yP.Index && xP.Name == yP.Name;
                }

                return ReferenceEquals(x, y);
            }

            #endregion
        }

        #endregion
    }
}