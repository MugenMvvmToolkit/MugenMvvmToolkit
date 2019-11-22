using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core
{
    public sealed class BindingParameterContext
    {
        #region Constructors

        public BindingParameterContext()
        {
            ComponentBuilders = new Dictionary<string, IBindingComponentBuilder>();
            EqualityParameters = new Dictionary<string, IExpressionNode>();
            InlineParameters = new Dictionary<string, bool>();
        }

        #endregion

        #region Properties

        public Dictionary<string, IBindingComponentBuilder> ComponentBuilders { get; }

        public Dictionary<string, IExpressionNode> EqualityParameters { get; }

        public Dictionary<string, bool> InlineParameters { get; }

        #endregion

        #region Methods

        public bool? TryGetBool(string parameterName)
        {
            Should.NotBeNull(parameterName, nameof(parameterName));
            var result = TryGetValue<bool?>(parameterName);
            if (result != null)
                return result;

            if (InlineParameters.TryGetValue(parameterName, out var value))
                return value;
            return null;
        }

        public TValue TryGetValue<TValue>(string parameterName, TValue defaultValue = default)
        {
            Should.NotBeNull(parameterName, nameof(parameterName));
            if (EqualityParameters.TryGetValue(parameterName, out var node))
            {
                if (node is IConstantExpressionNode constant && constant.Value is TValue value)
                    return value;
                BindingExceptionManager.ThrowCannotParseBindingParameter(parameterName, typeof(TValue).GetNonNullableType(), node);
            }
            return defaultValue;
        }

        public IExpressionNode? TryGetExpression(string parameterName)
        {
            Should.NotBeNull(parameterName, nameof(parameterName));
            EqualityParameters.TryGetValue(parameterName, out var node);
            return node;
        }

        public void Initialize<TList>(ItemOrList<IExpressionNode?, TList> parameters)
            where TList : class?, IReadOnlyList<IExpressionNode>
        {
            var list = parameters.List;
            if (list != null)
            {
                for (var i = 0; i < list.Count; i++)
                    AddParameter(list[i]);
            }
            else if (parameters.Item != null)
                AddParameter(parameters.Item);
        }

        public void Clear()
        {
            ComponentBuilders.Clear();
            EqualityParameters.Clear();
            InlineParameters.Clear();
        }

        public ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>> GetComponents()
        {
            if (ComponentBuilders.Count == 0)
                return default;
            if (ComponentBuilders.Count == 1)
                return new ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>>(ComponentBuilders.First().Value);
            return new ItemOrList<IBindingComponentBuilder?, IReadOnlyList<IBindingComponentBuilder>>(ComponentBuilders.Values.ToArray());
        }

        private void AddParameter(IExpressionNode expression)
        {
            switch (expression)
            {
                case IBindingComponentBuilder builder:
                    ComponentBuilders[builder.Name] = builder;
                    return;
                case IBinaryExpressionNode binary when binary.Token == BinaryTokenType.Equality && binary.Left is IMemberExpressionNode memberExpression:
                    EqualityParameters[memberExpression.MemberName] = binary.Right;
                    return;
                case IUnaryExpressionNode unary when unary.Token == UnaryTokenType.LogicalNegation && unary.Operand is IMemberExpressionNode member:
                    InlineParameters[member.MemberName] = false;
                    return;
                case IMemberExpressionNode m:
                    InlineParameters[m.MemberName] = true;
                    break;
            }
        }

        #endregion
    }
}