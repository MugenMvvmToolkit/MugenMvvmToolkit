using System;
using System.Collections.Generic;
using MugenMvvm.App;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Converting;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Commands;
using MugenMvvm.Components;
using MugenMvvm.Entities;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Serialization;
using MugenMvvm.Tests.Internal;
using MugenMvvm.Threading;
using MugenMvvm.Validation;
using MugenMvvm.ViewModels;
using MugenMvvm.Views;
using MugenMvvm.Wrapping;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests
{
    [Collection(SharedContext)]
    public class MugenServiceTest : UnitTestBase
    {
        [Fact]
        public void DefaultServicesShouldBeValid()
        {
            var services = new Dictionary<Type, object>
            {
                {typeof(IMugenApplication), new MugenApplication()},
                {typeof(ICommandManager), new CommandManager()},
                {typeof(IComponentCollectionManager), new ComponentCollectionManager()},
                {typeof(IAttachedValueManager), new AttachedValueManager()},
                {typeof(IReflectionManager), new ReflectionManager()},
                {typeof(IWeakReferenceManager), new WeakReferenceManager()},
                {typeof(IMessenger), new Messenger()},
                {typeof(IEntityManager), new EntityManager()},
                {typeof(INavigationDispatcher), new NavigationDispatcher()},
                {typeof(IPresenter), new Presenter()},
                {typeof(ISerializer), new Serializer()},
                {typeof(IThreadDispatcher), new ThreadDispatcher()},
                {typeof(IValidationManager), new ValidationManager()},
                {typeof(IViewModelManager), new ViewModelManager()},
                {typeof(IViewManager), new ViewManager()},
                {typeof(IWrapperManager), new WrapperManager()},
                {typeof(IGlobalValueConverter), new GlobalValueConverter()},
                {typeof(IBindingManager), new BindingManager()},
                {typeof(IMemberManager), new MemberManager()},
                {typeof(IObservationManager), new ObservationManager()},
                {typeof(IResourceManager), new ResourceManager()},
                {typeof(IExpressionParser), new ExpressionParser()},
                {typeof(IExpressionCompiler), new ExpressionCompiler()}
            };
            var fallback = new TestFallbackServiceConfiguration
            {
                Instance = type => services[type],
                Optional = type =>
                {
                    if (type == typeof(ILambdaExpressionCompiler) || type == typeof(ILockerProvider))
                        return null;
                    throw new NotSupportedException();
                }
            };
            MugenService.Configuration.Clear<MugenServiceTest>();
            MugenService.Configuration.Clear<ICommandManager>();
            MugenService.Configuration.Clear<IThreadDispatcher>();
            MugenService.Configuration.Clear<IComponentCollectionManager>();
            MugenService.Configuration.Clear<IAttachedValueManager>();
            MugenService.Configuration.Clear<IReflectionManager>();
            MugenService.Configuration.Clear<IWeakReferenceManager>();
            MugenService.Configuration.Clear<IMessenger>();
            MugenService.Configuration.Clear<IViewModelManager>();
            MugenService.Configuration.Clear<IGlobalValueConverter>();
            MugenService.Configuration.Clear<IBindingManager>();
            MugenService.Configuration.Clear<IObservationManager>();
            MugenService.Configuration.Clear<IMemberManager>();
            MugenService.Configuration.Clear<IResourceManager>();
            MugenService.Configuration.FallbackConfiguration = fallback;
            MugenService.Application.ShouldEqual(services[typeof(IMugenApplication)]);
            MugenService.CommandManager.ShouldEqual(services[typeof(ICommandManager)]);
            MugenService.ComponentCollectionManager.ShouldEqual(services[typeof(IComponentCollectionManager)]);
            MugenService.AttachedValueManager.ShouldEqual(services[typeof(IAttachedValueManager)]);
            MugenService.ReflectionManager.ShouldEqual(services[typeof(IReflectionManager)]);
            MugenService.WeakReferenceManager.ShouldEqual(services[typeof(IWeakReferenceManager)]);
            MugenService.Messenger.ShouldEqual(services[typeof(IMessenger)]);
            MugenService.EntityManager.ShouldEqual(services[typeof(IEntityManager)]);
            MugenService.NavigationDispatcher.ShouldEqual(services[typeof(INavigationDispatcher)]);
            MugenService.Presenter.ShouldEqual(services[typeof(IPresenter)]);
            MugenService.Serializer.ShouldEqual(services[typeof(ISerializer)]);
            MugenService.ThreadDispatcher.ShouldEqual(services[typeof(IThreadDispatcher)]);
            MugenService.ValidationManager.ShouldEqual(services[typeof(IValidationManager)]);
            MugenService.ViewModelManager.ShouldEqual(services[typeof(IViewModelManager)]);
            MugenService.ViewManager.ShouldEqual(services[typeof(IViewManager)]);
            MugenService.WrapperManager.ShouldEqual(services[typeof(IWrapperManager)]);
            MugenService.GlobalValueConverter.ShouldEqual(services[typeof(IGlobalValueConverter)]);
            MugenService.BindingManager.ShouldEqual(services[typeof(IBindingManager)]);
            MugenService.MemberManager.ShouldEqual(services[typeof(IMemberManager)]);
            MugenService.ObservationManager.ShouldEqual(services[typeof(IObservationManager)]);
            MugenService.ResourceManager.ShouldEqual(services[typeof(IResourceManager)]);
            MugenService.ExpressionParser.ShouldEqual(services[typeof(IExpressionParser)]);
            MugenService.ExpressionCompiler.ShouldEqual(services[typeof(IExpressionCompiler)]);
        }

        [Fact]
        public void InitializeInstanceShouldUseConstantValue()
        {
            MugenService.Configuration.Clear<MugenServiceTest>();
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();

            MugenService.Configuration.InitializeInstance(this);
            MugenService.Optional<MugenServiceTest>().ShouldEqual(this);
            MugenService.Instance<MugenServiceTest>().ShouldEqual(this);

            MugenService.Configuration.Clear<MugenServiceTest>();
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();
        }

        [Fact]
        public void InitializeInstanceShouldUseFallback()
        {
            MugenService.Configuration.Clear<MugenServiceTest>();
            MugenService.Configuration.FallbackConfiguration = null;
            var optional = new MugenServiceTest();
            var fallback = new TestFallbackServiceConfiguration
            {
                Instance = type =>
                {
                    type.ShouldEqual(GetType());
                    return this;
                },
                Optional = type =>
                {
                    if (type == typeof(ILambdaExpressionCompiler) || type == typeof(ILockerProvider))
                        return null;
                    type.ShouldEqual(GetType());
                    return optional;
                }
            };
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();

            MugenService.Configuration.FallbackConfiguration = fallback;
            MugenService.Configuration.FallbackConfiguration.ShouldEqual(fallback);
            MugenService.Optional<MugenServiceTest>().ShouldEqual(optional);
            MugenService.Instance<MugenServiceTest>().ShouldEqual(this);

            MugenService.Configuration.FallbackConfiguration = null;
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();
        }

        [Fact]
        public void InitializeInstanceShouldUseHasService()
        {
            MugenService.Configuration.Clear<MugenServiceTest>();
            var service = new TestHasServiceModel<MugenServiceTest> {Service = this, ServiceOptional = new MugenServiceTest()};
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();

            MugenService.Configuration.Initialize(service);
            MugenService.Optional<MugenServiceTest>().ShouldEqual(service.ServiceOptional);
            MugenService.Instance<MugenServiceTest>().ShouldEqual(service.Service);

            MugenService.Configuration.Clear<MugenServiceTest>();
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();
        }

        protected override void OnDispose() => MugenService.Configuration.FallbackConfiguration = null;
    }
}