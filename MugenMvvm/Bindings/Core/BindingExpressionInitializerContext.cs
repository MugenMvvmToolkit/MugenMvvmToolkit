using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Parsing.Expressions;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.Bindings.Core
{
    public class BindingExpressionInitializerContext : MetadataOwnerBase, IBindingExpressionInitializerContext
    {
        private object? _parameterExpressions;

        public BindingExpressionInitializerContext(object owner)
            : base(null)
        {
            Should.NotBeNull(owner, nameof(owner));
            Owner = owner;
            Target = null!;
            TargetExpression = null!;
            Components = new Dictionary<string, object?>();
            AssignmentParameters = new Dictionary<string, IExpressionNode>();
            InlineParameters = new Dictionary<string, bool>();
        }

        public object Owner { get; }

        public Dictionary<string, object?> Components { get; }

        public Dictionary<string, IExpressionNode> AssignmentParameters { get; }

        public Dictionary<string, bool> InlineParameters { get; }

        public object Target { get; private set; }

        public object? Source { get; private set; }

        public IExpressionNode TargetExpression { get; set; }

        public IExpressionNode? SourceExpression { get; set; }

        public ItemOrIReadOnlyList<IExpressionNode> ParameterExpressions
        {
            get => ItemOrIReadOnlyList.FromRawValue<IExpressionNode>(_parameterExpressions);
            set
            {
                _parameterExpressions = value.GetRawValue();
                InitializeParameters(value);
            }
        }

        IDictionary<string, object?> IBindingExpressionInitializerContext.Components => Components;

        public void Initialize(object target, object? source, IExpressionNode targetExpression, IExpressionNode? sourceExpression, ItemOrIReadOnlyList<IExpressionNode> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(targetExpression, nameof(targetExpression));
            AssignmentParameters.Clear();
            InlineParameters.Clear();
            Components.Clear();
            MetadataRaw?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
            Target = target;
            Source = source;
            TargetExpression = targetExpression;
            SourceExpression = sourceExpression;
            ParameterExpressions = parameters;
        }

        public void Clear()
        {
            Target = null!;
            Source = null;
            TargetExpression = null!;
            SourceExpression = null!;
            _parameterExpressions = null;
            Components.Clear();
            InlineParameters.Clear();
            AssignmentParameters.Clear();
            MetadataRaw?.Clear();
        }

        public TValue? TryGetParameterValue<TValue>(string parameterName, TValue? defaultValue = default)
        {
            Should.NotBeNull(parameterName, nameof(parameterName));
            if (AssignmentParameters.TryGetValue(parameterName, out var node))
            {
                if (node is TValue v)
                    return v;

                if (node is IConstantExpressionNode constant)
                {
                    if (constant.Value is TValue value)
                        return value;
                    return (TValue?) MugenService.GlobalValueConverter.Convert(constant.Value, typeof(TValue))!;
                }

                if (typeof(TValue) == typeof(string) && node is IMemberExpressionNode member)
                    return (TValue) (object) member.Member;

                ExceptionManager.ThrowCannotParseBindingParameter(parameterName, typeof(TValue).GetNonNullableType(), node);
            }

            if ((typeof(TValue) == typeof(bool?) || typeof(TValue) == typeof(bool)) && InlineParameters.TryGetValue(parameterName, out var b))
                return MugenExtensions.CastGeneric<bool, TValue>(b);

            return defaultValue;
        }

        private void InitializeParameters(ItemOrIReadOnlyList<IExpressionNode> parameters)
        {
            InlineParameters.Clear();
            AssignmentParameters.Clear();
            foreach (var parameter in parameters)
                AddParameter(parameter);
        }

        private void AddParameter(IExpressionNode expression)
        {
            switch (expression)
            {
                case IBinaryExpressionNode binary when binary.Token == BinaryTokenType.Assignment && binary.Left is IMemberExpressionNode memberExpression:
                    AssignmentParameters[memberExpression.Member] = binary.Right;
                    return;
                case IUnaryExpressionNode unary when unary.Token == UnaryTokenType.LogicalNegation && unary.Operand is IMemberExpressionNode member:
                    InlineParameters[member.Member] = false;
                    return;
                case IMemberExpressionNode m:
                    InlineParameters[m.Member] = true;
                    break;
            }
        }
    }
}