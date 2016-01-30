using Autofac;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.IoC;
using Should;

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

        [TestMethod]
        public override void ShouldResolveClassWithDynamicParameters()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var parameters = new IIocParameter[]
                {
                    new IocParameter("parameterConstructor", "parameterConstructor", IocParameterType.Constructor),
                };
                iocContainer.Bind(typeof(ParameterClass), typeof(ParameterClass), DependencyLifecycle.SingleInstance);
                var parameterClass = iocContainer.Get<ParameterClass>(parameters: parameters);
                parameterClass.ParameterConstructor.ShouldEqual(parameters[0].Value);
            }
        }

        #endregion
    }
}
