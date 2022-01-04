using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Converting;
using MugenMvvm.Bindings.Converting.Components;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Builders;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Parsing.Components;
using MugenMvvm.Bindings.Parsing.Components.Converters;
using MugenMvvm.Bindings.Parsing.Components.Parsers;
using MugenMvvm.Bindings.Parsing.Visitors;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        public static MugenApplicationConfiguration DefaultBindingConfiguration(this MugenApplicationConfiguration configuration, bool cacheResources = true)
        {
            var syncRoot = new object();
            configuration.WithAppService(MugenService.Optional<IExpressionCompiler>() ?? new ExpressionCompiler())
                         .WithComponent(new ExpressionCompilerCache())
                         .WithComponent(new CompiledExpressionCompiler())
                         .WithComponent(new BinaryExpressionBuilder())
                         .WithComponent(new ConditionExpressionBuilder())
                         .WithComponent(new ConstantExpressionBuilder())
                         .WithComponent(new LambdaExpressionBuilder())
                         .WithComponent(new MemberExpressionBuilder())
                         .WithComponent(new MethodCallIndexerExpressionBuilder())
                         .WithComponent(new NullConditionalExpressionBuilder())
                         .WithComponent(new ExpressionOptimizer())
                         .WithComponent(new UnaryExpressionBuilder())
                         .WithComponent(new SynchronizedExpressionCompilerDecorator(syncRoot));

            configuration.WithAppService(MugenService.Optional<IGlobalValueConverter>() ?? new GlobalValueConverter())
                         .WithComponent(new DefaultGlobalValueConverter());


            var managerCfg = configuration.WithAppService(MugenService.Optional<IBindingManager>() ?? new BindingManager())
                                          .WithComponent(new BindingBuilderDelegateExpressionParser())
                                          .WithComponent(new BindingExpressionExceptionDecorator())
                                          .WithComponent(new BindingExpressionParser())
                                          .WithComponent(new StringBindingExpressionCache())
                                          .WithComponent(new BindingExpressionPriorityDecorator())
                                          .WithComponent(new BindingHolder())
                                          .WithComponent(new BindingHolderLifecycleHandler())
                                          .WithComponent(new BindingExpressionInitializer())
                                          .WithComponent(new BindingModeInitializer())
                                          .WithComponent(new BindingParameterInitializer())
                                          .WithComponent(new InlineBindingExpressionInitializer())
                                          .WithComponent(new DelayBindingInitializer())
                                          .WithComponent(new SynchronizedBindingManagerDecorator(syncRoot));

            var macrosPreInitializer = managerCfg.Service.GetMacrosPreInitializer();
            var macrosVisitor = new MacrosExpressionVisitor();
            macrosPreInitializer.TargetVisitors.Add(macrosVisitor);
            macrosPreInitializer.SourceVisitors.Add(macrosVisitor);
            macrosPreInitializer.ParameterVisitors.Add(macrosVisitor);

            var macrosPostInitializer = managerCfg.Service.GetMacrosPostInitializer();
            var constantToBindingParameterVisitor = new ConstantToBindingParameterVisitor();
            macrosPostInitializer.SourceVisitors.Add(constantToBindingParameterVisitor);
            macrosPostInitializer.ParameterVisitors.Add(constantToBindingParameterVisitor);

            configuration.WithAppService(MugenService.Optional<IMemberManager>() ?? new MemberManager())
                         .WithComponent(new ExtensionMethodMemberProvider())
                         .WithComponent(new FakeMemberProvider())
                         .WithComponent(new IndexerAccessorMemberDecorator())
                         .WithComponent(new MemberCache())
                         .WithComponent(new MemberSelector())
                         .WithComponent(new MethodMemberAccessorDecorator())
                         .WithComponent(new MethodRequestMemberManagerDecorator())
                         .WithComponent(new NameRequestMemberManagerDecorator())
                         .WithComponent(new ReflectionMemberProvider())
                         .WithComponent(new SynchronizedMemberManagerDecorator(syncRoot));

            var cfg = configuration.WithAppService(MugenService.Optional<IObservationManager>() ?? new ObservationManager())
                                   .WithComponent(new EventInfoObserverProvider())
                                   .WithComponent(new EventMemberObserverProvider())
                                   .WithComponent(new MemberPathObserverProvider())
                                   .WithComponent(new MemberPathProvider())
                                   .WithComponent(new MemberPathProviderCache())
                                   .WithComponent(new NonObservableMemberObserverDecorator())
                                   .WithComponent(new PropertyChangedObserverProvider())
                                   .WithComponent(new SynchronizedObservationManagerDecorator(syncRoot));
            if (cacheResources)
                cfg.WithComponent(new ResourceMemberPathObserverCache());

            configuration.WithAppService(MugenService.Optional<IExpressionParser>() ?? new ExpressionParser())
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
                         .WithComponent(new ExpressionParserConverter())
                         .WithComponent(new BinaryExpressionConverter())
                         .WithComponent(new UnaryExpressionConverter())
                         .WithComponent(new ConstantExpressionConverter())
                         .WithComponent(new MemberExpressionConverter())
                         .WithComponent(new MethodCallExpressionConverter())
                         .WithComponent(new InvocationExpressionConverter())
                         .WithComponent(new ConditionExpressionConverter())
                         .WithComponent(new IndexerExpressionConverter())
                         .WithComponent(new LambdaExpressionConverter())
                         .WithComponent(new NewArrayExpressionConverter())
                         .WithComponent(new DefaultExpressionConverter())
                         .WithComponent(new SynchronizedExpressionParserDecorator(syncRoot));

            configuration.WithAppService(MugenService.Optional<IResourceManager>() ?? new ResourceManager())
                         .WithComponent(new ResourceResolver())
                         .WithComponent(new TypeResolver());

            return configuration;
        }

        public static MugenApplicationConfiguration AttachedMembersBaseConfiguration(this MugenApplicationConfiguration configuration)
        {
            var memberManager = configuration.GetService<IMemberManager>();
            var attachedMemberProvider = memberManager.GetAttachedMemberProvider();
            RegisterObjectAttachedMembers(attachedMemberProvider);
            RegisterValidationAttachedMembers(memberManager, attachedMemberProvider);
            return configuration;
        }

        public static void RegisterObjectAttachedMembers(AttachedMemberProvider attachedMemberProvider)
        {
            Should.NotBeNull(attachedMemberProvider, nameof(attachedMemberProvider));
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                                                   .DataContext()
                                                   .GetBuilder()
                                                   .Inherits()
                                                   .EqualityComparer(Default.ReferenceEqualityComparer)
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
                                                   .WithParameters(new[]
                                                   {
                                                       AttachedMemberBuilder.Parameter<string>().Build(),
                                                       AttachedMemberBuilder.Parameter<int>().DefaultValue(BoxingExtensions.Box(1)).Build()
                                                   })
                                                   .InvokeHandler((member, target, args, metadata) => FindRelativeSource(target, (string) args[0]!, (int) args[1]!, metadata))
                                                   .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                                                   .Parent()
                                                   .GetBuilder()
                                                   .WrapMember(nameof(Members.BindableMembers.ParentNative))
                                                   .Build());
        }

        public static void RegisterViewCollectionManagerMembers<T>(AttachedMemberProvider attachedMemberProvider, string? itemsSourceRawAlias = null) where T : class
        {
            Should.NotBeNull(attachedMemberProvider, nameof(attachedMemberProvider));
            attachedMemberProvider.Register(Members.BindableMembers.For<T>()
                                                   .ItemsSource()
                                                   .GetBuilder()
                                                   .CustomGetter((member, target, metadata) =>
                                                   {
                                                       var viewManager = MugenService.ViewManager;
                                                       if (!viewManager.GetComponents<IViewCollectionManagerComponent>(metadata)
                                                                       .TryGetItemsSource(viewManager, target, metadata, out var itemsSource))
                                                           ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                                                       return itemsSource;
                                                   })
                                                   .CustomSetter((member, target, value, metadata) =>
                                                   {
                                                       var viewManager = MugenService.ViewManager;
                                                       if (!viewManager.GetComponents<IViewCollectionManagerComponent>(metadata)
                                                                       .TrySetItemsSource(viewManager, target, value, metadata))
                                                           ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                                                       ((INotifiableMemberInfo) member).Raise(target, null, metadata);
                                                   })
                                                   .ObservableAutoHandler()
                                                   .Build());
            var itemsSourceRaw = Members.BindableMembers.For<T>()
                                        .ItemsSourceRaw()
                                        .GetBuilder()
                                        .CustomGetter((member, target, metadata) =>
                                        {
                                            var viewManager = MugenService.ViewManager;
                                            if (!viewManager.GetComponents<IViewCollectionManagerComponent>(metadata)
                                                            .TryGetItemsSourceRaw(viewManager, target, metadata, out var itemsSource))
                                                ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                                            return itemsSource;
                                        })
                                        .CustomSetter((member, target, value, metadata) =>
                                        {
                                            var viewManager = MugenService.ViewManager;
                                            if (!viewManager.GetComponents<IViewCollectionManagerComponent>(metadata)
                                                            .TrySetItemsSource(viewManager, target, value, metadata))
                                                ExceptionManager.ThrowInvalidBindingMember(target, member.Name);
                                            ((INotifiableMemberInfo) member).Raise(target, null, metadata);
                                        })
                                        .ObservableAutoHandler()
                                        .Build();
            attachedMemberProvider.Register(itemsSourceRaw);
            if (itemsSourceRawAlias != null)
                attachedMemberProvider.Register(itemsSourceRaw, itemsSourceRawAlias);
        }

        public static void RegisterValidationAttachedMembers(IMemberManager memberManager, AttachedMemberProvider attachedMemberProvider)
        {
            Should.NotBeNull(memberManager, nameof(memberManager));
            Should.NotBeNull(attachedMemberProvider, nameof(attachedMemberProvider));
            var errorsChangedEvent =
                memberManager.TryGetMember(typeof(INotifyDataErrorInfo), MemberType.Event, MemberFlags.InstancePublic, nameof(INotifyDataErrorInfo.ErrorsChanged));
            if (errorsChangedEvent != null)
            {
                attachedMemberProvider.Register(errorsChangedEvent, nameof(Members.BindableMembers.GetError) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(errorsChangedEvent, nameof(Members.BindableMembers.GetErrors) + BindingInternalConstant.ChangedEventPostfix);
                attachedMemberProvider.Register(errorsChangedEvent, nameof(Members.BindableMembers.HasErrors) + BindingInternalConstant.ChangedEventPostfix);
            }

            var errorsChangedEventValidator = AttachedMemberBuilder
                                              .Event<IHasService<IValidator>>(nameof(INotifyDataErrorInfo.ErrorsChanged))
                                              .CustomImplementation((member, target, listener, metadata) =>
                                              {
                                                  var component = new ErrorsChangedValidatorListener(listener);
                                                  target.GetService(false)!.AddComponent(component);
                                                  return ActionToken.FromDelegate((t, c) =>
                                                  {
                                                      var hasService = (IHasService<IValidator>?) ((IWeakReference) t!).Target;
                                                      hasService?.GetService(false)!.RemoveComponent((IComponent<IValidator>) c!);
                                                  }, target.ToWeakReference(), component);
                                              })
                                              .Build();
            attachedMemberProvider.Register(errorsChangedEventValidator);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.GetError) + BindingInternalConstant.ChangedEventPostfix);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.GetErrors) + BindingInternalConstant.ChangedEventPostfix);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.HasErrors) + BindingInternalConstant.ChangedEventPostfix);

            var stringParameter = AttachedMemberBuilder.Parameter("m", typeof(string)).DefaultValue("").Build();
            var stringsParameter = AttachedMemberBuilder.Parameter("m", typeof(string[])).IsParamsArray().Build();
            var hasErrorHandler = new InvokeMethodDelegate<IMethodMemberInfo, object, bool>((_, target, args, m) => HasErrors(target, args[0] ?? "", m));
            var getErrorHandler = new InvokeMethodDelegate<IMethodMemberInfo, object, object?>((_, target, args, m) => GetError(target, args[0] ?? "", m));
            var getErrorsHandler = new InvokeMethodDelegate<IMethodMemberInfo, object, IReadOnlyList<object>>((_, target, args, m) => GetErrors(target, args[0] ?? "", m));

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .HasErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(hasErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .HasErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(hasErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .HasErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(hasErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .HasErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(hasErrorHandler)
                                                   .Build());

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .GetErrorMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(getErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .GetErrorMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(getErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .GetErrorMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(getErrorHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .GetErrorMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(getErrorHandler)
                                                   .Build());

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .GetErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(getErrorsHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .GetErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringParameter)
                                                   .InvokeHandler(getErrorsHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                                                   .GetErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(getErrorsHandler)
                                                   .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                                                   .GetErrorsMethod()
                                                   .RawMethod
                                                   .GetBuilder()
                                                   .WithParameters(stringsParameter)
                                                   .InvokeHandler(getErrorsHandler)
                                                   .Build());
        }

        private sealed class ErrorsChangedValidatorListener : IValidatorErrorsChangedListener
        {
            private readonly WeakEventListener _eventListener;

            public ErrorsChangedValidatorListener(IEventListener eventListener)
            {
                _eventListener = eventListener.ToWeak();
            }

            public void OnErrorsChanged(IValidator validator, ItemOrIReadOnlyList<string> members, IReadOnlyMetadataContext? metadata)
            {
                if (!_eventListener.TryHandle(validator, members.GetRawValue() ?? "", metadata))
                    validator.RemoveComponent(this);
            }
        }
    }
}