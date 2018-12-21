using MugenMvvm.Interfaces;
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
            Singleton<IWeakReferenceFactory>.Initialize(new TestWeakReferenceFactory());
        }

        #endregion
    }
}