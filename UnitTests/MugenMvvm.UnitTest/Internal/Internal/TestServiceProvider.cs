using System;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestServiceProvider : IServiceProvider
    {
        #region Properties

        public Func<Type, object>? GetService { get; set; }

        #endregion

        #region Implementation of interfaces

        object IServiceProvider.GetService(Type serviceType)
        {
            return GetService?.Invoke(serviceType)!;
        }

        #endregion
    }
}