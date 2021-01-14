using System;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests
{
    public class MugenServiceTest : UnitTestBase
    {
        [Fact]
        public void DefaultServicesShouldBeValid()
        {
            Type? lastType = null;
            var fallback = new TestFallbackServiceConfiguration
            {
                Instance = type =>
                {
                    lastType = type;
                    return null!;
                },
                Optional = type => throw new NotSupportedException()
            };
            MugenService.Configuration.Clear<MugenServiceTest>();
            MugenService.Configuration.InitializeFallback(fallback);
            Validate(() => MugenService.Application, lastType);
            Validate(() => MugenService.CommandManager, lastType);
            Validate(() => MugenService.ComponentCollectionManager, lastType);
            Validate(() => MugenService.AttachedValueManager, lastType);
            Validate(() => MugenService.ReflectionManager, lastType);
            Validate(() => MugenService.WeakReferenceManager, lastType);
            Validate(() => MugenService.Messenger, lastType);
            Validate(() => MugenService.EntityManager, lastType);
            Validate(() => MugenService.NavigationDispatcher, lastType);
            Validate(() => MugenService.Presenter, lastType);
            Validate(() => MugenService.Serializer, lastType);
            Validate(() => MugenService.ThreadDispatcher, lastType);
            Validate(() => MugenService.ValidationManager, lastType);
            Validate(() => MugenService.ViewModelManager, lastType);
            Validate(() => MugenService.ViewManager, lastType);
            Validate(() => MugenService.WrapperManager, lastType);
            Validate(() => MugenService.GlobalValueConverter, lastType);
            Validate(() => MugenService.BindingManager, lastType);
            Validate(() => MugenService.MemberManager, lastType);
            Validate(() => MugenService.ObservationManager, lastType);
            Validate(() => MugenService.ResourceResolver, lastType);
            Validate(() => MugenService.ExpressionParser, lastType);
            Validate(() => MugenService.ExpressionCompiler, lastType);
        }

        [Fact]
        public void InitializeInstanceShouldUseConstantValue()
        {
            MugenService.Configuration.Clear<MugenServiceTest>();
            MugenService.Configuration.InitializeFallback(null);
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
            MugenService.Configuration.InitializeFallback(null);
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
                    type.ShouldEqual(GetType());
                    return optional;
                }
            };
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();

            MugenService.Configuration.InitializeFallback(fallback);
            MugenService.Configuration.GetFallbackConfiguration().ShouldEqual(fallback);
            MugenService.Optional<MugenServiceTest>().ShouldEqual(optional);
            MugenService.Instance<MugenServiceTest>().ShouldEqual(this);

            MugenService.Configuration.InitializeFallback(null);
            ShouldThrow<InvalidOperationException>(() => MugenService.Instance<MugenServiceTest>());
            MugenService.Optional<MugenServiceTest>().ShouldBeNull();
        }

        [Fact]
        public void InitializeInstanceShouldUseHasService()
        {
            MugenService.Configuration.Clear<MugenServiceTest>();
            MugenService.Configuration.InitializeFallback(null);
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

        private static void Validate<T>(Func<T> getService, in Type? type) where T : class
        {
            MugenService.Configuration.Clear<T>();
            getService();
            typeof(T).ShouldEqual(type);
        }
    }
}