using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;

namespace MugenMvvm.Binding.Core
{
    public class BindingExpressionInitializerContext : MetadataOwnerBase, IBindingExpressionInitializerContext
    {
        #region Fields

        private object? _parameters;

        #endregion

        #region Constructors

        public BindingExpressionInitializerContext(object owner, IMetadataContextProvider? metadataContextProvider)
            : base(null, metadataContextProvider)
        {
            Should.NotBeNull(owner, nameof(owner));
            Owner = owner;
            Target = null!;
            TargetExpression = null!;
            BindingComponents = new Dictionary<string, object?>();
            AssignmentParameters = new Dictionary<string, IExpressionNode>();
            InlineParameters = new Dictionary<string, bool>();
        }

        #endregion

        #region Properties

        public object Owner { get; }

        public object Target { get; private set; }

        public object? Source { get; private set; }

        public IExpressionNode TargetExpression { get; set; }

        public IExpressionNode? SourceExpression { get; set; }

        public ItemOrList<IExpressionNode, IList<IExpressionNode>> Parameters
        {
            get => ItemOrList<IExpressionNode, IList<IExpressionNode>>.FromRawValue(_parameters);
            set
            {
                _parameters = value.GetRawValue();
                InitializeParameters(value);
            }
        }

        IDictionary<string, object?> IBindingExpressionInitializerContext.BindingComponents => BindingComponents;

        public Dictionary<string, object?> BindingComponents { get; }

        public Dictionary<string, IExpressionNode> AssignmentParameters { get; }

        public Dictionary<string, bool> InlineParameters { get; }

        #endregion

        #region Implementation of interfaces

        public TValue TryGetParameterValue<TValue>(string parameterName, TValue defaultValue = default)
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
                    return (TValue)MugenBindingService.GlobalValueConverter.Convert(constant.Value, typeof(TValue))!;
                }

                if (typeof(TValue) == typeof(string) && node is IMemberExpressionNode member)
                    return (TValue)(object)member.Member;

                BindingExceptionManager.ThrowCannotParseBindingParameter(parameterName, typeof(TValue).GetNonNullableType(), node);
            }

            if ((typeof(TValue) == typeof(bool?) || typeof(TValue) == typeof(bool)) && InlineParameters.TryGetValue(parameterName, out var b))
                return MugenExtensions.CastGeneric<bool, TValue>(b);

            return defaultValue;
        }

        #endregion

        #region Methods

        public void Initialize(object target, object? source, IExpressionNode targetExpression, IExpressionNode? sourceExpression,
            ItemOrList<IExpressionNode, IList<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(targetExpression, nameof(targetExpression));
            AssignmentParameters.Clear();
            InlineParameters.Clear();
            BindingComponents.Clear();
            MetadataRaw?.Clear();
            if (!metadata.IsNullOrEmpty())
                Metadata.Merge(metadata!);
            Target = target;
            Source = source;
            TargetExpression = targetExpression;
            SourceExpression = sourceExpression;
            Parameters = parameters;
        }

        public void Clear()
        {
            Target = null!;
            Source = null;
            TargetExpression = null!;
            SourceExpression = null!;
            _parameters = null;
            BindingComponents.Clear();
            InlineParameters.Clear();
            AssignmentParameters.Clear();
            MetadataRaw?.Clear();
        }

        private void InitializeParameters(ItemOrList<IExpressionNode, IList<IExpressionNode>> parameters)
        {
            InlineParameters.Clear();
            AssignmentParameters.Clear();
            var list = parameters.List;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    AddParameter(list[i]);
            }
            else if (parameters.Item != null)
                AddParameter(parameters.Item);
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

        #endregion
    }
}