using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class MacrosBindingExpressionInterceptorComponent : IBindingExpressionInterceptorComponent, IHasPriority
    {
        #region Constructors

        public MacrosBindingExpressionInterceptorComponent()
        {
            Visitors = new List<IExpressionVisitor>();
        }

        #endregion

        #region Properties

        public List<IExpressionVisitor> Visitors { get; }

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        public bool IsCachePerTypeRequired => false;

        #endregion

        #region Implementation of interfaces

        public void Intercept(object target, object? source, ref IExpressionNode targetExpression, ref IExpressionNode sourceExpression, ref ItemOrList<IExpressionNode, List<IExpressionNode>> parameters,
            IReadOnlyMetadataContext? metadata)
        {
            //source is empty, target is expression
            if (sourceExpression is IMemberExpressionNode member && string.IsNullOrEmpty(member.Member)
                                                                 && !(targetExpression is IMemberExpressionNode)
                                                                 && !(targetExpression is IBindingMemberExpressionNode))
            {
                sourceExpression = targetExpression;
                targetExpression = new MemberExpressionNode(null, FakeMemberProviderComponent.FakeMemberPrefixSymbol + Default.NextCounter().ToString());
            }

            for (int i = 0; i < Visitors.Count; i++)
            {
                var visitor = Visitors[i];
                targetExpression = targetExpression.Accept(visitor, metadata);
                sourceExpression = sourceExpression.Accept(visitor, metadata);
                for (int j = 0; j < parameters.Count(); j++)
                    parameters.Set(parameters.Get(j).Accept(visitor, metadata), j);
            }
        }

        #endregion
    }
}