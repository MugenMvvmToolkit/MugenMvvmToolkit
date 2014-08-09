using System;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.IoC;
using Ninject;
using Should;

namespace MugenMvvmToolkit.Test.Ioc
{
    [TestClass]
    public class NinjectContainerTest : IocContainerTestBase<NinjectContainer>
    {
        #region Constructors

        public NinjectContainerTest()
            : base(() => new NinjectContainer(new StandardKernel()))
        {
        }

        #endregion

        #region Overrides of IocContainerTestBase<NinjectIocAdapter>

        [TestMethod]
        public override void TestBindToTypeNamed()
        {
            const string named = "named";

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldNotEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldNotEqual(simple);
            }
        }

        [TestMethod]
        public override void TestBindToMethodNamed()
        {
            const string named = "named";
            var parameters = new IIocParameter[] { new IocParameter("test", "Test", new IocParameterType("Test")) };

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod<ISimple>((adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named, parameters).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named, parameters).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod(typeof(ISimple), (adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named, parameters).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named, parameters).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod<ISimple>((adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple) iocContainer.Get(typeof (ISimple), named, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named, parameters).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named, parameters).ShouldNotEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod(typeof(ISimple), (adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named, parameters).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named, parameters).ShouldNotEqual(simple);
            }
        }

        [TestMethod]
        public override void TestBindToConstantNamed()
        {
            const string named = "named";

            using (IIocContainer iocContainer = GetIocContainer())
            {
                var constValue = new Simple();
                iocContainer.BindToConstant<ISimple>(constValue, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                simple.ShouldEqual(constValue);
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                var constValue = new Simple();
                iocContainer.BindToConstant(typeof(ISimple), constValue, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                simple.ShouldEqual(constValue);
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);
            }
        }

        [TestMethod]
        public override void TestUnbind()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var constValue = new Simple();
                iocContainer.BindToConstant<ISimple>(constValue);

                iocContainer.Get<ISimple>().ShouldEqual(constValue);

                iocContainer.Unbind(typeof(ISimple));
                Action action = () => iocContainer.Get<ISimple>().ShouldEqual(constValue);
                action.ShouldThrow();
            }
        }

        #endregion
    }
}