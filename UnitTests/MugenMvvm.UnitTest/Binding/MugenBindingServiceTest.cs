using System;
using MugenMvvm.Binding;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding
{
    public class MugenBindingServiceTest
    {
        #region Methods

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
            Validate(() => MugenBindingService.GlobalValueConverter, lastType);
            Validate(() => MugenBindingService.BindingManager, lastType);
            Validate(() => MugenBindingService.MemberManager, lastType);
            Validate(() => MugenBindingService.ObservationManager, lastType);
            Validate(() => MugenBindingService.ResourceResolver, lastType);
            Validate(() => MugenBindingService.Parser, lastType);
            Validate(() => MugenBindingService.Compiler, lastType);
        }

        private static void Validate<T>(Func<T> getService, in Type? type) where T : class
        {
            MugenService.Configuration.Clear<T>();
            getService();
            typeof(T).ShouldEqual(type);
        }

        #endregion
    }
}