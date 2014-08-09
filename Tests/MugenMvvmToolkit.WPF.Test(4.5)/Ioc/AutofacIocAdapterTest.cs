using Autofac;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MugenMvvmToolkit.Test.Ioc
{
    [TestClass]
    public class AutofacContainerTest : IocContainerTestBase<AutofacContainer>
    {
        #region Constructors

        public AutofacContainerTest()
            : base(() => new AutofacContainer(new ContainerBuilder()))
        {
        }

        #endregion

        #region Overrides of IocContainerTestBase<AutofacIocAdapter>

        public override void TestUnbind()
        {
            //Not supported
        }

        #endregion
    }
}