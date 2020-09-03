using MugenMvvm.App.Configuration;
using MugenMvvm.Binding.Build.Components;
using MugenMvvm.Binding.Compiling;
using MugenMvvm.Binding.Compiling.Components;
using MugenMvvm.Binding.Convert;
using MugenMvvm.Binding.Convert.Components;
using MugenMvvm.Binding.Core;
using MugenMvvm.Binding.Core.Components;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Builders;
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

namespace MugenMvvm.Binding.Extensions
{
    public static partial class MugenBindingExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration DefaultBindingConfiguration(this MugenApplicationConfiguration configuration)
        {
            configuration.WithAppService(new ExpressionCompiler())
                .WithComponent(new BinaryExpressionBuilder())
                .WithComponent(new ConditionExpressionBuilder())
                .WithComponent(new ConstantExpressionBuilder())
                .WithComponent(new ExpressionCompilerComponent())
                .WithComponent(new LambdaExpressionBuilder())
                .WithComponent(new MemberExpressionBuilder())
                .WithComponent(new MethodCallIndexerExpressionBuilder())
                .WithComponent(new NullConditionalExpressionBuilder())
                .WithComponent(new UnaryExpressionBuilder());

            configuration.WithAppService(new GlobalValueConverter())
                .WithComponent(new GlobalValueConverterComponent());

            var macrosBindingInitializer = new MacrosBindingInitializer();
            var macrosVisitor = new MacrosExpressionVisitor();
            macrosBindingInitializer.TargetVisitors.Add(macrosVisitor);
            macrosBindingInitializer.SourceVisitors.Add(macrosVisitor);
            macrosBindingInitializer.ParameterVisitors.Add(macrosVisitor);

            configuration.WithAppService(new BindingManager())
                .WithComponent(macrosBindingInitializer)
                .WithComponent(new BindingBuilderListExpressionParser())
                .WithComponent(new BindingBuilderDelegateExpressionParser())
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
                .WithComponent(new InlineExpressionBindingInitializer())
                .WithComponent(new DelayBindingInitializer());

            configuration.WithAppService(new MemberManager())
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

            configuration.WithAppService(new ObservationManager())
                .WithComponent(new EventInfoMemberObserverProvider())
                .WithComponent(new EventMemberObserverProvider())
                .WithComponent(new MemberPathObserverProvider())
                .WithComponent(new MemberPathProvider())
                .WithComponent(new MemberPathProviderCache())
                .WithComponent(new PropertyChangedMemberObserverProvider());

            configuration.WithAppService(new ExpressionParser())
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

            configuration.WithAppService(new ResourceResolver())
                .WithComponent(new ResourceResolverComponent())
                .WithComponent(new TypeResolverComponent());

            return configuration;
        }

        public static MugenApplicationConfiguration AttachedMembersBaseConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.ServiceConfiguration<IMemberManager>().Service();
            var attachedMemberProvider = memberManager.GetOrAddComponent(context => new AttachedMemberProvider());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                .DataContext()
                .GetBuilder()
                .Inherits()
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                .Root()
                .GetBuilder()
                .CustomGetter((member, target, metadata) => GetRoot(target, metadata))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                .RelativeSourceMethod()
                .RawMethod
                .GetBuilder(false)
                .WithParameters(AttachedMemberBuilder.Parameter<string>("p1").Build(), AttachedMemberBuilder.Parameter<string>("p1").DefaultValue(BoxingExtensions.Box(1)).Build())
                .InvokeHandler((member, target, args, metadata) => FindRelativeSource(target, (string) args[0]!, (int) args[1]!, metadata))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                .Parent()
                .GetBuilder()
                .WrapMember(Members.BindableMembers.For<object>().ParentNative())
                .Build());
            return configuration;
        }

        #endregion
    }
}