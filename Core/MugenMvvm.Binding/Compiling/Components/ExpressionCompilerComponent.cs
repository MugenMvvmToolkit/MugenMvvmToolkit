using System;
using System.Linq.Expressions;
using System.Reflection;
using MugenMvvm.Binding.Interfaces.Compiling;
using MugenMvvm.Binding.Interfaces.Compiling.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Interfaces.Parsing.Expressions;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Compiling.Components
{
    public class ExpressionCompilerComponent : IExpressionCompilerComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker<IExpressionBuilder, IExpressionCompiler> _componentTracker;
        private readonly IMetadataContextProvider? _metadataContextProvider;

        #endregion

        #region Constructors

        public ExpressionCompilerComponent(IMetadataContextProvider? metadataContextProvider = null)
        {
            _metadataContextProvider = metadataContextProvider;
            _componentTracker = new ComponentTracker<IExpressionBuilder, IExpressionCompiler>();
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public ICompiledExpression? TryCompile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return Compile(expression, metadata);
        }

        #endregion

        #region Methods

        protected virtual ICompiledExpression Compile(IExpressionNode expression, IReadOnlyMetadataContext? metadata)
        {
            return null;
        }

        #endregion

        #region Nested types

        public interface IContext : IMetadataOwner<IMetadataContext>
        {
            ParameterExpression MetadataExpression { get; }

            MethodInfo? CurrentLambdaMethod { get; } //todo keep prev value

            void SetCurrentLambdaParameter(IBindingParameterInfo parameter);

            void ClearCurrentLambdaParameter(IBindingParameterInfo parameter);

            ParameterExpression? GetParameterExpression(IParameterExpression parameterExpression);

            void SetParameterExpression(IParameterExpression parameterExpression, ParameterExpression? value);

            Expression Build(IExpressionNode expression);
        }

        public interface IExpressionBuilder : IComponent<IExpressionCompiler>
        {
            Expression? TryBuild(IContext context, IExpressionNode expression);
        }

        #endregion
    }
}