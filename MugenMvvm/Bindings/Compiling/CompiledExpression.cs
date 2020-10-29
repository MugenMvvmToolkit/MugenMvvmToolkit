using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Compiling
{
    public sealed class CompiledExpression : ICompiledExpression, IExpressionBuilderContext, IExpressionVisitor, IEqualityComparer<IExpressionNode>, IEqualityComparer<object>
    {
        #region Fields

        private readonly Dictionary<object, Func<object?[], object?>> _cache;
        private readonly IExpressionNode _expression;
        private readonly Dictionary<IExpressionNode, Expression?> _expressions;
        private readonly IReadOnlyMetadataContext? _inputMetadata;

        private IExpressionBuilderComponent[] _expressionBuilders;
        private IMetadataContext? _metadata;
        private object?[] _values;

        private static readonly object?[] DisposedValues = new object?[0];
        private static readonly ParameterExpression[] ArrayParameterArray = {MugenExtensions.GetParameterExpression<object[]>()};

        #endregion

        #region Constructors

        public CompiledExpression(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            _inputMetadata = metadata;
            _cache = new Dictionary<object, Func<object?[], object?>>(this);
            _expressions = new Dictionary<IExpressionNode, Expression?>(this);
            _expression = expression.Accept(this, metadata);
            _values = new object[_expressions.Count + 1];
            _expressionBuilders = Default.Array<IExpressionBuilderComponent>();
            MetadataExpression = MugenExtensions.GetIndexExpression(_expressions.Count).ConvertIfNeed(typeof(IReadOnlyMetadataContext), false);
        }

        #endregion

        #region Properties

        public bool HasMetadata => !(_metadata ?? _inputMetadata).IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata ?? MugenExtensions.EnsureInitialized(ref _metadata, new MetadataContext());

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
            if (_values == DisposedValues)
                ExceptionManager.ThrowObjectDisposed(this);
            var list = values.List;
            var key = list ?? values.Item.Type ?? (object) Default.Array<ParameterValue>();
            if (!_cache.TryGetValue(key, out var invoker))
            {
                invoker = CompileExpression(values);
                if (list == null)
                    _cache[key] = invoker;
                else
                {
                    var types = new Type[list.Length];
                    for (var i = 0; i < list.Length; i++)
                        types[i] = list[i].Type;
                    _cache[types] = invoker;
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
            if (_values == DisposedValues)
                return;
            _values = DisposedValues;
            _expressions.Clear();
            _cache.Clear();
        }

        int IEqualityComparer<IExpressionNode>.GetHashCode([AllowNull] IExpressionNode key)
        {
            if (key is IBindingMemberExpressionNode member)
                return HashCode.Combine(member.Index, member.Path);
            return RuntimeHelpers.GetHashCode(key!);
        }

        bool IEqualityComparer<IExpressionNode>.Equals([AllowNull] IExpressionNode x, [AllowNull] IExpressionNode y) =>
            x == y || x is IBindingMemberExpressionNode xP && y is IBindingMemberExpressionNode yP && xP.Index == yP.Index && xP.Path == yP.Path;

        bool IEqualityComparer<object>.Equals([AllowNull] object x, [AllowNull] object y)
        {
            if (x == y)
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
                return Equals(typesY!, (ParameterValue[]) x!);
            if (typesY == null)
                return Equals(typesX!, (ParameterValue[]) y!);

            if (typesX.Length != typesY.Length)
                return false;
            for (var i = 0; i < typesX.Length; i++)
            {
                if (typesX[i] != typesY[i])
                    return false;
            }

            return true;
        }

        int IEqualityComparer<object>.GetHashCode([AllowNull] object key)
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
                var types = (Type[]) key!;
                for (var index = 0; index < types.Length; index++)
                    hashCode.Add(types[index]);
            }

            return hashCode.ToHashCode();
        }

        public Expression? TryGetExpression(IExpressionNode expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressions.TryGetValue(expression, out var value);
            return value;
        }

        public void SetExpression(IExpressionNode expression, Expression value)
        {
            Should.NotBeNull(expression, nameof(expression));
            Should.NotBeNull(value, nameof(value));
            _expressions[expression] = value;
        }

        public void ClearExpression(IExpressionNode expression)
        {
            Should.NotBeNull(expression, nameof(expression));
            _expressions.Remove(expression);
        }

        public Expression? TryBuild(IExpressionNode expression) => _expressionBuilders.TryBuild(this, expression) ?? TryGetExpression(expression);

        IExpressionNode IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IBindingMemberExpressionNode memberExpression)
            {
                if (memberExpression.Index < 0)
                {
                    this.TryGetErrors()?.Add(BindingMessageConstant.CannotCompileBindingMemberExpressionFormat2.Format(memberExpression, memberExpression.Index));
                    this.ThrowCannotCompile(memberExpression);
                }

                _expressions[memberExpression] = null;
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
                var memberValues = ItemOrListEditor.Get<KeyValuePair<IBindingMemberExpressionNode, Expression>>(pair => pair.Key == null);
                foreach (var value in _expressions)
                {
                    if (!(value.Key is IBindingMemberExpressionNode memberExpression))
                        continue;

                    var index = MugenExtensions.GetIndexExpression(memberExpression.Index);
                    if (expressionValues == null)
                    {
                        if (memberExpression.Index != 0)
                            ExceptionManager.ThrowIndexOutOfRangeCollection(nameof(values));
                        memberValues.Add(new KeyValuePair<IBindingMemberExpressionNode, Expression>(memberExpression, index.ConvertIfNeed(values.Item.Type, false)));
                    }
                    else
                        memberValues.Add(new KeyValuePair<IBindingMemberExpressionNode, Expression>(memberExpression, index.ConvertIfNeed(expressionValues[memberExpression.Index].Type, false)));
                }

                foreach (var pair in memberValues.ToItemOrList().Iterator(pair => pair.Key == null))
                    _expressions[pair.Key] = pair.Value;

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
    }
}