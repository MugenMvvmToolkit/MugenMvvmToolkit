using MugenMvvm.Binding.Build.Components;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Convert;
using MugenMvvm.Binding.Convert.Components;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Components;
using MugenMvvm.Binding.Parsing;
using MugenMvvm.Binding.Parsing.Components;
using MugenMvvm.Binding.Parsing.Components.Converters;
using MugenMvvm.Binding.Parsing.Components.Parsers;
using MugenMvvm.Binding.Parsing.Visitors;
using MugenMvvm.Binding.Resources;
using MugenMvvm.Binding.Resources.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        public static IMugenApplication DefaultBindingConfiguration(this IMugenApplication application)
        {
            Should.NotBeNull(application, nameof(application));
            application
                .WithService(new ExpressionCompiler())
                .WithService(new GlobalValueConverter())
                .WithService(new BindingManager())
                .WithService(new MemberManager())
                .WithService(new ObservationManager())
                .WithService(new ExpressionParser())
                .WithService(new ResourceResolver());

            MugenBindingService
                .Compiler
                .WithComponent(new BinaryExpressionBuilder())
                .WithComponent(new ConditionExpressionBuilder())
                .WithComponent(new ConstantExpressionBuilder())
                .WithComponent(new ExpressionCompilerComponent())
                .WithComponent(new LambdaExpressionBuilder())
                .WithComponent(new MemberExpressionBuilder())
                .WithComponent(new MethodCallIndexerExpressionBuilder())
                .WithComponent(new NullConditionalExpressionBuilder())
                .WithComponent(new UnaryExpressionBuilder());
            MugenBindingService
                .GlobalValueConverter
                .WithComponent(new GlobalValueConverterComponent());
            var macrosBindingInitializer = new MacrosBindingInitializer();
            var macrosVisitor = new MacrosExpressionVisitor();
            macrosBindingInitializer.TargetVisitors.Add(macrosVisitor);
            macrosBindingInitializer.SourceVisitors.Add(macrosVisitor);
            macrosBindingInitializer.ParameterVisitors.Add(macrosVisitor);
            MugenBindingService
                .BindingManager
                .WithComponent(macrosBindingInitializer)
                .WithComponent(new BindingBuilderListExpressionParser())
                .WithComponent(new BindingBuilderRequestExpressionParser())
                .WithComponent(new BindingCleaner())
                .WithComponent(new BindingExpressionExceptionDecorator())
                .WithComponent(new BindingExpressionParser())
                .WithComponent(new BindingExpressionParserCache())
                .WithComponent(new BindingExpressionPriorityDecorator())
                .WithComponent(new BindingHolder())
                .WithComponent(new BindingHolderLifecycleDispatcher())
                .WithComponent(new BindingInitializer())
                .WithComponent(new BindingModeInitializer())
                .WithComponent(new BindingParameterInitializer())
                .WithComponent(new DelayBindingInitializer());
            MugenBindingService.MemberManager
                .WithComponent(new AttachedDynamicMemberProvider())
                .WithComponent(new AttachedMemberProvider())
                .WithComponent(new ExtensionMethodMemberProvider())
                .WithComponent(new FakeMemberProvider())
                .WithComponent(new IndexerAccessorMemberDecorator())
                .WithComponent(new MemberManagerCache())
                .WithComponent(new MemberSelector())
                .WithComponent(new MethodMemberAccessorDecorator())
                .WithComponent(new MethodRequestMemberManagerDecorator())
                .WithComponent(new NameRequestMemberManagerDecorator())
                .WithComponent(new ReflectionMemberProvider());
            MugenBindingService.ObservationManager
                .WithComponent(new EventInfoMemberObserverProvider())
                .WithComponent(new EventMemberObserverProvider())
                .WithComponent(new MemberPathObserverProvider())
                .WithComponent(new MemberPathProvider())
                .WithComponent(new MemberPathProviderCache())
                .WithComponent(new PropertyChangedMemberObserverProvider());
            MugenBindingService.Parser
                .WithComponent(new ExpressionParserComponent())
                .WithComponent(new UnaryTokenParser())
                .WithComponent(new MemberTokenParser())
                .WithComponent(new BinaryTokenParser())
                .WithComponent(new ParenTokenParser())
                .WithComponent(new ConditionTokenParser())
                .WithComponent(new DigitTokenParser())
                .WithComponent(new MethodCallTokenParser())
                .WithComponent(new ConstantTokenParser())
                .WithComponent(new IndexerTokenParser())
                .WithComponent(new LambdaTokenParser())
                .WithComponent(new StringTokenParser())
                .WithComponent(new NullConditionalMemberTokenParser())
                .WithComponent(new AssignmentTokenParser())
                .WithComponent(new ExpressionConverterComponent())
                .WithComponent(new BinaryExpressionConverter())
                .WithComponent(new UnaryExpressionConverter())
                .WithComponent(new ConstantExpressionConverter())
                .WithComponent(new MemberExpressionConverter())
                .WithComponent(new MethodCallExpressionConverter())
                .WithComponent(new ConditionExpressionConverter())
                .WithComponent(new IndexerExpressionConverter())
                .WithComponent(new NewArrayExpressionConverter())
                .WithComponent(new DefaultExpressionConverter());
            MugenBindingService.ResourceResolver
                .WithComponent(new ResourceResolverComponent())
                .WithComponent(new TypeResolverComponent());

            return application;
        }

        #endregion
    }
}