using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingActionExpressionInterceptorComponent : IBindingExpressionInterceptorComponent, IHasPriority
    {
        #region Properties

        public int Priority { get; set; } = int.MaxValue;

        #endregion

        #region Implementation of interfaces

        public void Intercept(ref IExpressionNode targetExpression, ref IExpressionNode sourceExpression, ref ItemOrList<IExpressionNode?, List<IExpressionNode>> parameters, IReadOnlyMetadataContext? metadata)
        {
            //source is empty, target is expression
            if (sourceExpression is IMemberExpressionNode member && string.IsNullOrEmpty(member.MemberName)
                                                                 && !(targetExpression is IMemberExpressionNode)
                                                                 && !(targetExpression is IBindingMemberExpressionNode))
            {
                sourceExpression = targetExpression;
                targetExpression = new MemberExpressionNode(null, FakeMemberProviderComponent.FakeMemberPrefixSymbol + Default.NextCounter().ToString());
            }
        }

        #endregion
    }
}