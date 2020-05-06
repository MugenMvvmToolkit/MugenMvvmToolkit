using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Parsing.Expressions;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class MacrosBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Constructors

        public MacrosBindingInitializer()
        {
            Visitors = new List<IExpressionVisitor>();
        }

        #endregion

        #region Properties

        public List<IExpressionVisitor> Visitors { get; }

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
        {
            //source is empty, target is expression
            if (context.SourceExpression is IMemberExpressionNode member && string.IsNullOrEmpty(member.Member)
                                                                         && !(context.TargetExpression is IMemberExpressionNode)
                                                                         && !(context.TargetExpression is IBindingMemberExpressionNode))
            {
                context.SourceExpression = context.TargetExpression;
                context.TargetExpression = new MemberExpressionNode(null, FakeMemberProvider.FakeMemberPrefixSymbol + Default.NextCounter().ToString());
            }

            if (Visitors.Count == 0)
                return;

            var metadata = context.GetMetadataOrDefault();
            var parameters = context.Parameters;
            for (var i = 0; i < Visitors.Count; i++)
            {
                var visitor = Visitors[i];
                context.TargetExpression = context.TargetExpression.Accept(visitor, metadata);
                context.SourceExpression = context.SourceExpression?.Accept(visitor, metadata);
                for (var j = 0; j < parameters.Count(); j++)
                    parameters.Set(parameters.Get(j).Accept(visitor, metadata), j);
            }

            context.Parameters = parameters;
        }

        #endregion
    }
}