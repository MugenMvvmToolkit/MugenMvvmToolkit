using MugenMvvm.Infrastructure;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.UnitTest.TestInfrastructure;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace MugenMvvm.UnitTest
{
    public class UnitTestBase
    {
        #region Constructors

        public UnitTestBase()
        {
            UnifiedReflectionApiInitializer.Initialize();
            Singleton<IWeakReferenceFactory>.Initialize(new TestWeakReferenceFactory());
            Singleton<IThreadDispatcher>.Initialize(new TestThreadDispatcher());
            Singleton<IReflectionManager>.Initialize(new ExpressionReflectionManager());
        }

        #endregion
    }
}