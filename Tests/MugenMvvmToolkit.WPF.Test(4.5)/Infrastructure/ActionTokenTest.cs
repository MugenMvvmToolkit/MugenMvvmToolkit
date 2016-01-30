using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Infrastructure;
using Should;

namespace MugenMvvmToolkit.Test.Infrastructure
{
    [TestClass]
    public class ActionTokenTest
    {
        [TestMethod]
        public void MethodShouldBeCalledOnce()
        {
            int count = 0;
            var actionToken = new ActionToken(() => count++);
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(1);
        }

        [TestMethod]
        public void MethodShouldBeCalledOnceWithParameter()
        {
            int count = 0;
            var actionToken = new ActionToken(o => count++, count);
            actionToken.Dispose();
            count.ShouldEqual(1);
            actionToken.Dispose();
            count.ShouldEqual(1);
        }
    }
}
