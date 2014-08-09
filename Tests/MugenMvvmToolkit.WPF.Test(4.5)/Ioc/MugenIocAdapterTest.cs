using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenInjection;

namespace MugenMvvmToolkit.Test.Ioc
{
    [TestClass]
    public class MugenContainerTest : IocContainerTestBase<MugenContainer>
    {
        #region Constructors

        public MugenContainerTest()
            : base(() => new MugenContainer(new MugenInjector()))
        {
        }

        #endregion
    }
}