using System.Collections.Generic;
using MugenMvvm.Binding.Constants;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Core
{
    public sealed class ModeBindingComponentProvider : BindingComponentProviderComponent.IProvider, IExpressionVisitor
    {
        #region Fields

        private readonly Dictionary<string, IBindingComponentBuilder> _bindingModes;

        #endregion

        #region Constructors

        public ModeBindingComponentProvider()
        {
            _bindingModes = new Dictionary<string, IBindingComponentBuilder>();
        }

        #endregion

        #region Properties

        public bool IsPostOrder => false;

        public IDictionary<string, IBindingComponentBuilder> BindingModes => _bindingModes;

        #endregion

        #region Implementation of interfaces

        IExpressionNode? IExpressionVisitor.Visit(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            if (expression is IBindingMemberExpressionNode memberExpression)
                memberExpression.Flags &= ~(BindingMemberExpressionFlags.Observable | BindingMemberExpressionFlags.ObservableMethod);
            return expression;
        }

        public void Initialize(IExpressionNode targetExpression, IExpressionNode sourceExpression, BindingParameterContext context, IReadOnlyMetadataContext? metadata)
        {
            if (context.ComponentBuilders.ContainsKey(BindingParameterNameConstants.EventHandler) && !(sourceExpression is IBindingMemberExpressionNode))
            {
                sourceExpression.Accept(this, metadata);
                context.ComponentBuilders[BindingParameterNameConstants.Mode] = InstanceBindingComponentBuilder.NoneMode;
                return;
            }

            if (context.ComponentBuilders.ContainsKey(BindingParameterNameConstants.Mode))
                return;

            if (context.EqualityParameters.TryGetValue(BindingParameterNameConstants.Mode, out var expression))
            {
                if (expression is IMemberExpressionNode memberExpression && _bindingModes.TryGetValue(memberExpression.MemberName, out var mode))
                    context.ComponentBuilders[BindingParameterNameConstants.Mode] = mode;
                else
                    BindingExceptionManager.ThrowCannotParseBindingParameter(BindingParameterNameConstants.Mode, string.Join(",", _bindingModes.Keys), expression);
                return;
            }

            foreach (var builder in _bindingModes)
            {
                if (context.InlineParameters.TryGetValue(builder.Key, out var value) && value)
                {
                    context.ComponentBuilders[BindingParameterNameConstants.Mode] = builder.Value;
                    return;
                }
            }
        }

        #endregion
    }
}