using System;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class FallbackServiceConfigurationTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void InstanceShouldReturnServiceProvider()
        {
            var serviceProvider = new TestServiceProvider();
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            fallbackServiceConfiguration.Instance<IServiceProvider>().ShouldEqual(serviceProvider);
        }

        [Fact]
        public void OptionalShouldReturnServiceProvider()
        {
            var serviceProvider = new TestServiceProvider();
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            fallbackServiceConfiguration.Optional<IServiceProvider>().ShouldEqual(serviceProvider);
        }

        [Fact]
        public void InstanceShouldThrowNullValue()
        {
            var serviceProvider = new TestServiceProvider();
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            ShouldThrow<InvalidOperationException>(() => fallbackServiceConfiguration.Instance<FallbackServiceConfigurationTest>());
        }

        [Fact]
        public void OptionalShouldReturnNullValue()
        {
            var serviceProvider = new TestServiceProvider();
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            fallbackServiceConfiguration.Optional<FallbackServiceConfigurationTest>().ShouldBeNull();
        }

        [Fact]
        public void InstanceShouldReturnValueFromServiceProvider()
        {
            var count = 0;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.GetService = type =>
            {
                ++count;
                type.ShouldEqual(typeof(FallbackServiceConfigurationTest));
                return this;
            };
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            fallbackServiceConfiguration.Instance<FallbackServiceConfigurationTest>().ShouldEqual(this);
            count.ShouldEqual(1);
        }

        [Fact]
        public void OptionalShouldReturnValueFromServiceProvider()
        {
            var count = 0;
            var serviceProvider = new TestServiceProvider();
            serviceProvider.GetService = type =>
            {
                ++count;
                type.ShouldEqual(typeof(FallbackServiceConfigurationTest));
                return this;
            };
            var fallbackServiceConfiguration = new FallbackServiceConfiguration(serviceProvider);
            fallbackServiceConfiguration.Optional<FallbackServiceConfigurationTest>().ShouldEqual(this);
            count.ShouldEqual(1);
        }

        #endregion
    }
}