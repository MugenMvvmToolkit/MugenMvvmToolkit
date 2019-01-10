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
            ServiceConfiguration<IWeakReferenceFactory>.Initialize(new TestWeakReferenceFactory());
            ServiceConfiguration<IThreadDispatcher>.Initialize(new TestThreadDispatcher());
            ServiceConfiguration<IReflectionManager>.Initialize(new ExpressionReflectionManager());
        }

        #endregion
    }
}