using System.Collections.Generic;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Parsing;
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
            TargetVisitors = new List<IExpressionVisitor>();
            SourceVisitors = new List<IExpressionVisitor>();
            ParameterVisitors = new List<IExpressionVisitor>();
        }

        #endregion

        #region Properties

        public List<IExpressionVisitor> TargetVisitors { get; }

        public List<IExpressionVisitor> SourceVisitors { get; }

        public List<IExpressionVisitor> ParameterVisitors { get; }

        public int Priority { get; set; } = ComponentPriority.PreInitializer;

        #endregion

        #region Implementation of interfaces

        public void Initialize(IBindingExpressionInitializerContext context)
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
                var parameters = context.Parameters;
                for (var i = 0; i < ParameterVisitors.Count; i++)
                {
                    var visitor = ParameterVisitors[i];
                    for (var j = 0; j < parameters.Count(); j++)
                        parameters.Set(parameters.Get(j).Accept(visitor, metadata), j);
                }

                context.Parameters = parameters;
            }
        }

        #endregion
    }
}