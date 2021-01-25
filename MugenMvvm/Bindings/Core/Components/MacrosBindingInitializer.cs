using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class MacrosBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        [Preserve(Conditional = true)]
        public MacrosBindingInitializer()
        {
            TargetVisitors = new List<IExpressionVisitor>();
            SourceVisitors = new List<IExpressionVisitor>();
            ParameterVisitors = new List<IExpressionVisitor>();
        }

        public List<IExpressionVisitor> TargetVisitors { get; }

        public List<IExpressionVisitor> SourceVisitors { get; }

        public List<IExpressionVisitor> ParameterVisitors { get; }

        public int Priority { get; set; }

        public void Initialize(IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            var metadata = context.GetMetadataOrDefault();
            for (var i = 0; i < TargetVisitors.Count; i++)
                context.TargetExpression = context.TargetExpression.Accept(TargetVisitors[i], metadata);

            if (context.SourceExpression != null)
            {
                for (var i = 0; i < SourceVisitors.Count; i++)
                    context.SourceExpression = context.SourceExpression?.Accept(SourceVisitors[i], metadata);
            }

            if (ParameterVisitors.Count != 0)
                context.VisitParameterExpressions(ParameterVisitors, metadata);
        }
    }
}