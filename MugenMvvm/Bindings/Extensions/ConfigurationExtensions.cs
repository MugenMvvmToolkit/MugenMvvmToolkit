using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Compiling.Components;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Convert;
using MugenMvvm.Bindings.Convert.Components;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Core.Components;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
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
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.Validation.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions
{
    public static partial class BindingMugenExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration DefaultBindingConfiguration(this MugenApplicationConfiguration configuration, bool cacheResources = true)
        {
            configuration.WithAppService(new ExpressionCompiler())
                .WithComponent(new ExpressionCompilerCache())
                .WithComponent(new ExpressionCompilerComponent())
                .WithComponent(new BinaryExpressionBuilder())
                .WithComponent(new ConditionExpressionBuilder())
                .WithComponent(new ConstantExpressionBuilder())
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
            var constantToBindingParameterVisitor = new ConstantToBindingParameterVisitor();
            macrosBindingInitializer.SourceVisitors.Add(constantToBindingParameterVisitor);
            macrosBindingInitializer.ParameterVisitors.Add(constantToBindingParameterVisitor);

            configuration.WithAppService(new BindingManager())
                .WithComponent(macrosBindingInitializer)
                .WithComponent(new BindingBuilderDelegateExpressionParser())
                .WithComponent(new BindingCleaner())
                .WithComponent(new BindingExpressionExceptionDecorator())
                .WithComponent(new BindingExpressionParser())
                .WithComponent(new BindingExpressionParserCache())
                .WithComponent(new BindingExpressionPriorityDecorator())
                .WithComponent(new BindingHolder())
                .WithComponent(new BindingHolderLifecycleHandler())
                .WithComponent(new BindingExpressionInitializer())
                .WithComponent(new BindingModeInitializer())
                .WithComponent(new BindingParameterInitializer())
                .WithComponent(new InlineBindingExpressionInitializer())
                .WithComponent(new DelayBindingInitializer());

            configuration.WithAppService(new MemberManager())
                .WithComponent(new AttachedMemberProvider())
                .WithComponent(new ExtensionMethodMemberProvider())
                .WithComponent(new FakeMemberProvider())
                .WithComponent(new IndexerAccessorMemberDecorator())
                .WithComponent(new MemberCache())
                .WithComponent(new MemberSelector())
                .WithComponent(new MethodMemberAccessorDecorator())
                .WithComponent(new MethodRequestMemberManagerDecorator())
                .WithComponent(new NameRequestMemberManagerDecorator())
                .WithComponent(new ReflectionMemberProvider());

            var cfg = configuration.WithAppService(new ObservationManager())
                .WithComponent(new EventInfoMemberObserverProvider())
                .WithComponent(new EventMemberObserverProvider())
                .WithComponent(new MemberPathObserverProvider())
                .WithComponent(new MemberPathProvider())
                .WithComponent(new MemberPathProviderCache())
                .WithComponent(new PropertyChangedMemberObserverProvider());
            if (cacheResources)
                cfg.WithComponent(new ResourceMemberPathObserverCache());

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
                .WithComponent(new LambdaExpressionConverter())
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
            RegisterObjectAttachedMembers(attachedMemberProvider);
            RegisterCollectionAttachedMembers(attachedMemberProvider);
            RegisterValidationAttachedMembers(memberManager, attachedMemberProvider);
            return configuration;
        }

        public static void RegisterObjectAttachedMembers(AttachedMemberProvider attachedMemberProvider)
        {
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
                .WithParameters(AttachedMemberBuilder.Parameter<string>().Build(), AttachedMemberBuilder.Parameter<string>().DefaultValue(BoxingExtensions.Box(1)).Build())
                .InvokeHandler((member, target, args, metadata) => FindRelativeSource(target, (string) args[0]!, (int) args[1]!, metadata))
                .ObservableHandler((member, target, listener, metadata) => RootSourceObserver.GetOrAdd(target).Add(listener))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<object>()
                .Parent()
                .GetBuilder()
                .WrapMember(Members.BindableMembers.For<object>().ParentNative())
                .Build());
        }

        public static void RegisterCollectionAttachedMembers(AttachedMemberProvider attachedMemberProvider)
        {
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Event<IComponentOwner<ICollection>>(nameof(ICollection.Count) + BindingInternalConstant.ChangedEventPostfix)
                .CustomImplementation((member, target, listener, metadata) => BindingCollectionAdapter.GetOrAdd(target).Listeners.Add(listener))
                .Build());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Event<IComponentOwner<ICollection>>(BindingInternalConstant.IndexerGetterName + BindingInternalConstant.ChangedEventPostfix)
                .CustomImplementation((member, target, listener, metadata) => BindingCollectionAdapter.GetOrAdd(target).Listeners.Add(listener))
                .Build());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Property<IObservableCollection, int>(nameof(IObservableCollection.Count))
                .CustomGetter((member, target, metadata) => BindingCollectionAdapter.GetOrAdd(target).Count)
                .Build());
            attachedMemberProvider.Register(AttachedMemberBuilder
                .Method<IObservableCollection, object?>(BindingInternalConstant.IndexerGetterName)
                .WithParameters(AttachedMemberBuilder.Parameter<int>().Build())
                .InvokeHandler((member, target, args, metadata) => BindingCollectionAdapter.GetOrAdd(target)[(int) args[0]!])
                .Build());
        }

        public static void RegisterValidationAttachedMembers(IMemberManager memberManager, AttachedMemberProvider attachedMemberProvider)
        {
            var errorsChangedEvent = memberManager.TryGetMember(typeof(INotifyDataErrorInfo), MemberType.Event, MemberFlags.InstancePublic, nameof(INotifyDataErrorInfo.ErrorsChanged));
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
                    target.Service.AddComponent(component);
                    return new ActionToken((t, c) =>
                    {
                        var hasService = (IHasService<IValidator>?) ((IWeakReference) t!).Target;
                        hasService?.Service.RemoveComponent((IComponent<IValidator>) c!);
                    }, target.ToWeakReference(), component);
                })
                .Build();
            attachedMemberProvider.Register(errorsChangedEventValidator);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.GetError) + BindingInternalConstant.ChangedEventPostfix);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.GetErrors) + BindingInternalConstant.ChangedEventPostfix);
            attachedMemberProvider.Register(errorsChangedEventValidator, nameof(Members.BindableMembers.HasErrors) + BindingInternalConstant.ChangedEventPostfix);

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                .HasErrorsMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => HasErrors(target, (string[]) args[0]!))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                .HasErrorsMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => HasErrors(target, (string[]) args[0]!))
                .Build());

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                .GetErrorMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => GetError(target, (string[]) args[0]!))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                .GetErrorMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => GetError(target, (string[]) args[0]!))
                .Build());

            attachedMemberProvider.Register(Members.BindableMembers.For<INotifyDataErrorInfo>()
                .GetErrorsMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => GetErrors(target, (string[]) args[0]!))
                .Build());
            attachedMemberProvider.Register(Members.BindableMembers.For<IHasService<IValidator>>()
                .GetErrorsMethod()
                .GetBuilder()
                .InvokeHandler((member, target, args, metadata) => GetErrors(target, (string[]) args[0]!))
                .Build());
        }

        #endregion

        #region Nested types

        private sealed class BindingCollectionAdapter : BindableCollectionAdapter, IComponent<ICollection>
        {
            #region Constructors

            private BindingCollectionAdapter()
            {
                Listeners = new EventListenerCollection();
            }

            #endregion

            #region Properties

            public EventListenerCollection Listeners { get; }

            #endregion

            #region Methods

            public static BindingCollectionAdapter GetOrAdd(IComponentOwner<ICollection> collection)
                => collection.GetOrAddComponent(collection, (owner, context) => new BindingCollectionAdapter {Collection = (IEnumerable) owner});

            protected override void BatchUpdate(List<CollectionChangedEvent> events, int version)
            {
                base.BatchUpdate(events, version);
                Raise();
            }

            protected override void OnAdded(object? item, int index, bool batchUpdate, int version)
            {
                base.OnAdded(item, index, batchUpdate, version);
                if (!batchUpdate)
                    Raise();
            }

            protected override void OnRemoved(object? item, int index, bool batchUpdate, int version)
            {
                base.OnRemoved(item, index, batchUpdate, version);
                if (!batchUpdate)
                    Raise();
            }

            protected override void OnMoved(object? item, int oldIndex, int newIndex, bool batchUpdate, int version)
            {
                base.OnMoved(item, oldIndex, newIndex, batchUpdate, version);
                if (!batchUpdate)
                    Raise();
            }

            protected override void OnReplaced(object? oldItem, object? newItem, int index, bool batchUpdate, int version)
            {
                base.OnReplaced(oldItem, newItem, index, batchUpdate, version);
                if (!batchUpdate)
                    Raise();
            }

            protected override void OnReset(IEnumerable<object?>? items, bool batchUpdate, int version)
            {
                base.OnReset(items, batchUpdate, version);
                if (!batchUpdate)
                    Raise();
            }

            private void Raise() => Listeners.Raise(Collection, EventArgs.Empty, null);

            #endregion
        }

        private sealed class ErrorsChangedValidatorListener : IValidatorListener
        {
            #region Fields

            private readonly WeakEventListener _eventListener;

            #endregion

            #region Constructors

            public ErrorsChangedValidatorListener(IEventListener eventListener)
            {
                _eventListener = eventListener.ToWeak();
            }

            #endregion

            #region Implementation of interfaces

            public void OnErrorsChanged(IValidator validator, object? target, string memberName, IReadOnlyMetadataContext? metadata)
            {
                if (!_eventListener.TryHandle(target, memberName, metadata))
                    validator.RemoveComponent(this);
            }

            public void OnAsyncValidation(IValidator validator, object? target, string memberName, Task validationTask, IReadOnlyMetadataContext? metadata)
            {
            }

            public void OnDisposed(IValidator validator)
            {
            }

            #endregion
        }

        #endregion
    }
}