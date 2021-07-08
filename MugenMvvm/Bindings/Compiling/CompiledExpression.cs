using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Compiling.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Compiling
{
    public sealed class CompiledExpression : ICompiledExpression, IExpressionBuilderContext, IExpressionVisitor, IEqualityComparer<IExpressionNode>, IEqualityComparer<object>
    {
        private static readonly ParameterExpression[] ArrayParameterArray = { MugenExtensions.GetParameterExpression<object[]>() };

        private readonly Dictionary<object, Func<object?[], object?>> _cache;
        private readonly IExpressionNode _expression;
        private readonly Dictionary<IExpressionNode, Expression?> _expressions;
        private readonly IReadOnlyMetadataContext? _inputMetadata;
        private readonly object?[] _values;

        private object? _expressionBuilders;
        private IMetadataContext? _metadata;

        public CompiledExpression(IExpressionNode expression, IReadOnlyMetadataContext? metadata = null)
        {
            _inputMetadata = metadata;
            _cache = new Dictionary<object, Func<object?[], object?>>(this);
            _expressions = new Dictionary<IExpressionNode, Expression?>(this);
            _expression = expression.Accept(this, metadata);
            _values = new object[_expressions.Count + 1];
            MetadataExpression = MugenExtensions.GetIndexExpression(_expressions.Count).ConvertIfNeed(typeof(IReadOnlyMetadataContext), false);
        }

        public ItemOrArray<IExpressionBuilderComponent> ExpressionBuilders
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ItemOrArray.FromRawValue<IExpressionBuilderComponent>(_expressionBuilders);
            set => _expressionBuilders = value.GetRawValue();
        }

        public Expression MetadataExpression { get; }

        public bool HasMetadata => !(_metadata ?? _inputMetadata).IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata ?? MugenExtensions.EnsureInitialized(ref _metadata, new MetadataContext(_inputMetadata));

        ExpressionTraversalType IExpressionVisitor.TraversalType => ExpressionTraversalType.Preorder;

        public object? Invoke(ItemOrArray<ParameterValue> values, IReadOnlyMetadataContext? metadata)
        {
            var key = values.List ?? values.Item.Type ?? (object)Array.Empty<Type>();
            if (!_cache.TryGetValue(key, out var invoker))
            {
                invoker = CompileExpression(values);
                if (values.List == null)
                    _cache[key] = invoker;
                else
                {
                    var types = new Type[values.List.Length];
                    for (var i = 0; i < values.List.Length; i++)
                        types[i] = values.List[i].Type;
                    _cache[types] = invoker;
                }
            }

            if (values.List == null)
                _values[0] = values.Item.Value;
            else
            {
                for (var i = 0; i < values.List.Length; i++)
                    _values[i] = values.List[i].Value;
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
            if (expression is not IBindingMemberExpressionNode)
                _expressions.Remove(expression);
        }

        public Expression? TryBuild(IExpressionNode expression) => ExpressionBuilders.TryBuild(this, expression) ?? TryGetExpression(expression);

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

        private Func<object?[], object?> CompileExpression(ItemOrArray<ParameterValue> values)
        {
            try
            {
                var memberValues = new ItemOrListEditor<KeyValuePair<IBindingMemberExpressionNode, Expression>>(2);
                foreach (var value in _expressions)
                {
                    if (value.Key is not IBindingMemberExpressionNode memberExpression)
                        continue;

                    var index = MugenExtensions.GetIndexExpression(memberExpression.Index);
                    memberValues.Add(new KeyValuePair<IBindingMemberExpressionNode, Expression>(memberExpression, index.ConvertIfNeed(values[memberExpression.Index].Type, false)));
                }

                foreach (var pair in memberValues)
                    _expressions[pair.Key] = pair.Value;

                return Expression
                       .Lambda<Func<object?[], object?>>(this.Build(_expression).ConvertIfNeed(typeof(object), false), ArrayParameterArray)
                       .CompileEx();
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

        int IEqualityComparer<IExpressionNode>.GetHashCode(IExpressionNode key)
        {
            if (key is IBindingMemberExpressionNode member)
                return HashCode.Combine(member.Index, member.Path);
            return RuntimeHelpers.GetHashCode(key!);
        }

        bool IEqualityComparer<IExpressionNode>.Equals(IExpressionNode? x, IExpressionNode? y) =>
            x == y || x is IBindingMemberExpressionNode xP && y is IBindingMemberExpressionNode yP && xP.Index == yP.Index && xP.Path == yP.Path;

        bool IEqualityComparer<object>.Equals(object? x, object? y)
        {
            if (x == y)
                return true;

            if (x is Type t)
            {
                if (y is Type yT)
                    return t == yT;
                return false;
            }

            var typesX = x as Type[];
            var typesY = y as Type[];
            if (typesX == null && typesY == null)
                return false;

            if (typesX == null)
                return Equals(typesY!, (ParameterValue[])x!);
            if (typesY == null)
                return Equals(typesX!, (ParameterValue[])y!);
            return InternalEqualityComparer.Equals(typesX, typesY);
        }

        int IEqualityComparer<object>.GetHashCode(object key)
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
                foreach (var t in (Type[])key!)
                    hashCode.Add(t);
            }

            return hashCode.ToHashCode();
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

                _expressions[memberExpression] = null;
            }

            return expression;
        }
    }
}