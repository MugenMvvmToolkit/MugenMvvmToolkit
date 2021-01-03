using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class MacrosBindingInitializer : IBindingExpressionInitializerComponent, IHasPriority
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public MacrosBindingInitializer()
        {
            TargetVisitors = new List<IExpressionVisitor>();
            SourceVisitors = new List<IExpressionVisitor>();
            ParameterVisitors = new List<IExpressionVisitor>();
        }

        #endregion

        #region Properties

        public List<IExpressionVisitor> TargetVisitors { get; }

        public List<IExpressionVisitor> SourceVisitors { get; }

        public List<IExpressionVisitor> ParameterVisitors { get; }

        public int Priority { get; set; } = BindingComponentPriority.ParameterPreInitializer;

        #endregion

        #region Implementation of interfaces

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
            {
                var parameters = context.ParameterExpressions.Editor();
                for (var i = 0; i < ParameterVisitors.Count; i++)
                {
                    var visitor = ParameterVisitors[i];
                    for (var j = 0; j < parameters.Count; j++)
                        parameters[j] = parameters[j].Accept(visitor, metadata);
                }

                context.ParameterExpressions = parameters.ToItemOrList();
            }
        }

        #endregion
    }
}