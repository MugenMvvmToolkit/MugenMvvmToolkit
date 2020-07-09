using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Compiling
{
    public sealed class CompiledExpression : LightDictionary<object, Func<object?[], object?>>, ICompiledExpression, IExpressionBuilderContext, IExpressionVisitor
    {
        #region Fields

        private static readonly object?[] DisposedValues = new object?[0];

        private readonly IExpressionNode _expression;
        private readonly ExpressionDictionary _expressionsDict;
        private readonly IReadOnlyMetadataContext? _inputMetadata;
        private readonly IMetadataContextManager? _metadataContextManager;
        private object?[] _values;

        private IExpressionBuilderComponent[] _expressionBuilders;
        private IMetadataContext? _metadata;

        private static readonly ParameterExpression[] ArrayParameterArray = { MugenExtensions.GetParameterExpression<object[]>() };

        #endregion

        #region Constructors

        public CompiledExpression(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null, IMetadataContextManager? metadataContextManager = null)
        {
            _inputMetadata = metadata;
            _metadataContextManager = metadataContextManager;
            _expressionsDict = new ExpressionDictionary();
            _expression = expression.Accept(this, metadata);
            _values = new object[_expressionsDict.Count + 1];
            _expressionBuilders = Default.Array<IExpressionBuilderComponent>();
            MetadataExpression = MugenExtensions.GetIndexExpression(_expressionsDict.Count).ConvertIfNeed(typeof(IReadOnlyMetadataContext), false);
        }

        #endregion

        #region Properties

        public bool HasMetadata => !(_metadata ?? _inputMetadata).IsNullOrEmpty();

        public IMetadataContext Metadata
        {
            get
            {
                if (_metadata == null)
                    _metadataContextManager.LazyInitialize(ref _metadata, this, _inputMetadata);
                return _metadata;
            }
        }

        public Expression MetadataExpression { get; }

        public IExpressionBuilderComponent[] ExpressionBuilders
        {
            get => _expressionBuilders;
            set
            {
                Should.NotBeNull(value, nameof(value));
                _expressionBuilders = value;
            }
        }

        bool IExpressionVisitor.IsPostOrder => false;

        #endregion

        #region Implementation of interfaces

        public object? Invoke(ItemOrList<ParameterValue, ParameterValue[]> values, IReadOnlyMetadataContext? metadata)
        {
            if (ReferenceEquals(_values, DisposedValues))
                ExceptionManager.ThrowObjectDisposed(this);
            var list = values.List;
            var key = list ?? values.Item.Type ?? (object)Default.Array<ParameterValue>();
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

        public void Dispose()
        {
            if (ReferenceEquals(_values, DisposedValues))
                return;
            _values = DisposedValues;
            _expressionsDict.Clear();
            Clear();
        }

        public Expression? TryGetExpression(IExpressionNode expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressionsDict.TryGetValue(expression, out var value);
            return value;
        }

        public void SetExpression(IExpressionNode expression, Expression value)
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(value, nameof(value));
            _expressionsDict[expression] = value;
        }

        public void ClearExpression(IExpressionNode expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressionsDict.Remove(expression);
        }

        public Expression? TryBuild(IExpressionNode expression)
        {
            return _expressionBuilders.TryBuild(this, expression) ?? TryGetExpression(expression);
        }

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IBindingMemberExpressionNode memberExpression)
            {
                if (memberExpression.Index < 0)
                {
                    this.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileBindingMemberExpressionFormat2.Format(memberExpression, memberExpression.Index));
                    this.ThrowCannotCompile(memberExpression);
                }

                _expressionsDict[memberExpression] = null;
            }

            return expression;
        }

        #endregion

        #region Methods

        private Func<object?[], object?> CompileExpression(ItemOrList<ParameterValue, ParameterValue[]> values)
        {
            try
            {
                var expressionValues = values.List;
                foreach (var value in _expressionsDict)
                {
                    if (!(value.Key is IBindingMemberExpressionNode memberExpression))
                        continue;

                    var index = MugenExtensions.GetIndexExpression(memberExpression.Index);
                    if (expressionValues == null)
                    {
                        if (memberExpression.Index != 0)
                            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(values));
                        _expressionsDict[memberExpression] = index.ConvertIfNeed(values.Item.Type, false);
                    }
                    else
                        _expressionsDict[memberExpression] = index.ConvertIfNeed(expressionValues[memberExpression.Index].Type, false);
                }

                var expression = this.Build(_expression).ConvertIfNeed(typeof(object), false);
                var lambda = Expression.Lambda<Func<object?[], object?>>(expression, ArrayParameterArray);
                return lambda.CompileEx();
            }
            finally
            {
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
            if (ReferenceEquals(x, y))
                return true;

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
                return false;

            if (typesX == null)
                return Equals(typesY!, (ParameterValue[])x);
            if (typesY == null)
                return Equals(typesX!, (ParameterValue[])y);

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

            var hashCode = new HashCode();
            if (key is ParameterValue[] values)
            {
                for (var index = 0; index < values.Length; index++)
                    hashCode.Add(values[index].Type);
            }
            else
            {
                var types = (Type[])key;
                for (var index = 0; index < types.Length; index++)
                    hashCode.Add(types[index]);
            }

            return hashCode.ToHashCode();
        }

        private static bool Equals(Type[] types, ParameterValue[] values)
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

        #endregion

        #region Nested types

        private sealed class ExpressionDictionary : LightDictionary<IExpressionNode, Expression?>
        {
            #region Constructors

            public ExpressionDictionary() : base(3)
            {
            }

            #endregion

            #region Methods

            protected override int GetHashCode(IExpressionNode key)
            {
                if (key is IBindingMemberExpressionNode member)
                    return HashCode.Combine(member.Index, member.Path);
                return RuntimeHelpers.GetHashCode(key);
            }

            protected override bool Equals(IExpressionNode x, IExpressionNode y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                return x is IBindingMemberExpressionNode xP && y is IBindingMemberExpressionNode yP && xP.Index == yP.Index && xP.Path == yP.Path;
            }

            #endregion
        }

        #endregion
    }
}