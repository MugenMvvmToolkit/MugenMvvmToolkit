#region Copyright

// ****************************************************************************
// <copyright file="IocAdapterTestBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models.IoC;
using Should;

namespace MugenMvvmToolkit.Test.Ioc
{
    [TestClass]
    public abstract class IocContainerTestBase<T> where T : IIocContainer
    {
        #region Fields

        protected readonly Func<T> GetIocContainer;

        #endregion

        #region Constructors

        protected IocContainerTestBase(Func<T> getIocContainer)
        {
            GetIocContainer = getIocContainer;
        }

        #endregion

        #region Test methods

        [TestMethod]
        public virtual void TestCreateChildAndGetRoot()
        {
            using (var iocContainer = GetIocContainer())
            {
                iocContainer.ShouldEqual(iocContainer.GetRoot());

                var child = iocContainer.CreateChild();
                iocContainer.ShouldNotEqual(child);
                child.Container.ShouldNotBeNull();
                child.GetRoot().ShouldEqual(iocContainer);
            }
        }

        [TestMethod]
        public virtual void TestSelfBindable()
        {
            using (var iocContainer = GetIocContainer())
            {
                var o = iocContainer.Get(typeof(Simple));
                o.ShouldNotBeNull();
                var simple = iocContainer.Get<Simple>();
                simple.ShouldNotBeNull();
                simple.ShouldNotEqual(o);
            }
        }

        [TestMethod]
        public virtual void TestComplexConstructor()
        {
            using (var iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.SingleInstance);
                var simple = iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();

                var complexDependency = iocContainer.Get<ComplexDependency>();
                complexDependency.Simple.ShouldEqual(simple);
                complexDependency.Simples.Single().ShouldEqual(simple);
            }
        }

        [TestMethod]
        public virtual void TestGetAll()
        {
            using (var iocContainer = GetIocContainer())
            {
                var simples = new List<ISimple>();
                for (int i = 0; i < 10; i++)
                {
                    simples.Add(new Simple());
                    iocContainer.BindToConstant(simples[i]);
                }
                var items = iocContainer.GetAll<ISimple>();
                foreach (var simple in items)
                    simples.Remove(simple);
                simples.ShouldBeEmpty();
            }
        }

        [TestMethod]
        public virtual void TestBindToType()
        {
            using (var iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.SingleInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>().ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.SingleInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>().ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.TransientInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>().ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldNotEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.TransientInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>().ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldNotEqual(simple);
            }
        }

        [TestMethod]
        public virtual void TestBindToMethod()
        {
            var parameters = new IIocParameter[] { new IocParameter("test", "Test", new IocParameterType("Test")) };
            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod<ISimple>((adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.SingleInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), null, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(null, parameters).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), null, parameters).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod(typeof(ISimple), (adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.SingleInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), null, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(null, parameters).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), null, parameters).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod<ISimple>((adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.TransientInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), null, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(null, parameters).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), null, parameters).ShouldNotEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod(typeof(ISimple), (adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.TransientInstance);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), null, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(null, parameters).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), null, parameters).ShouldNotEqual(simple);
            }
        }


        [TestMethod]
        public virtual void TestBindToConstant()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var constValue = new Simple();
                iocContainer.BindToConstant<ISimple>(constValue);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                simple.ShouldEqual(constValue);
                iocContainer.Get<ISimple>().ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldEqual(simple);
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                var constValue = new Simple();
                iocContainer.BindToConstant(typeof(ISimple), constValue);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple));
                simple.ShouldNotBeNull();
                simple.ShouldEqual(constValue);
                iocContainer.Get<ISimple>().ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple)).ShouldEqual(simple);
            }
        }

        [TestMethod]
        public virtual void TestBindToTypeNamed()
        {
            const string named = "named";

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.SingleInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldEqual(simple);

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind<ISimple, Simple>(DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldNotEqual(simple);

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.Bind(typeof(ISimple), typeof(Simple), DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named).ShouldNotEqual(simple);

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }
        }

        [TestMethod]
        public virtual void TestBindToMethodNamed()
        {
            const string named = "named";
            var parameters = new IIocParameter[] { new IocParameter("test", "Test", IocParameterType.Property) };

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

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
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

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }

            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.BindToMethod<ISimple>((adapter, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.TransientInstance, named);

                var simple = (ISimple)iocContainer.Get(typeof(ISimple), named, parameters);
                simple.ShouldNotBeNull();
                iocContainer.Get<ISimple>(named, parameters).ShouldNotEqual(simple);
                iocContainer.Get(typeof(ISimple), named, parameters).ShouldNotEqual(simple);

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
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

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }
        }

        [TestMethod]
        public virtual void TestBindToConstantNamed()
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

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
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

                Action action = () => iocContainer.Get<ISimple>();
                action.ShouldThrow();
            }
        }

        [TestMethod]
        public virtual void TestUnbind()
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

        [TestMethod]
        public virtual void TestCanResolve()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.CanResolve(typeof(ISimple)).ShouldBeFalse();
                iocContainer.CanResolve<ISimple>().ShouldBeFalse();

                iocContainer.BindToConstant<ISimple>(new Simple());

                iocContainer.CanResolve(typeof(ISimple)).ShouldBeTrue();
                iocContainer.CanResolve<ISimple>().ShouldBeTrue();
            }
        }

        [TestMethod]
        public virtual void TestCanResolveNamed()
        {
            const string name = "name";
            using (IIocContainer iocContainer = GetIocContainer())
            {
                iocContainer.CanResolve(typeof(ISimple), name).ShouldBeFalse();
                iocContainer.CanResolve<ISimple>(name).ShouldBeFalse();

                iocContainer.BindToConstant<ISimple>(new Simple());

                iocContainer.CanResolve(typeof(ISimple), name).ShouldBeFalse();
                iocContainer.CanResolve<ISimple>(name).ShouldBeFalse();

                iocContainer.BindToConstant<ISimple>(new Simple(), name);

                iocContainer.CanResolve(typeof(ISimple), name).ShouldBeTrue();
                iocContainer.CanResolve<ISimple>(name).ShouldBeTrue();
            }
        }

        [TestMethod]
        public virtual void ShouldPassPredefinedParametersToMethodDelegate()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var parameters = new IIocParameter[]
                {
                    new IocParameter("parameterConstructor", "parameterConstructor", IocParameterType.Constructor),
                    new IocParameter("ParameterProperty", "ParameterProperty", IocParameterType.Property)
                };
                iocContainer.BindToMethod(typeof(Simple), (container, list) =>
                {
                    list.SequenceEqual(parameters).ShouldBeTrue();
                    return new Simple();
                }, DependencyLifecycle.SingleInstance, parameters: parameters);
                iocContainer.Get<Simple>();
            }
        }

        [TestMethod]
        public virtual void ShouldResolveClassWithDynamicParameters()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var parameters = new IIocParameter[]
                {
                    new IocParameter("parameterConstructor", "parameterConstructor", IocParameterType.Constructor),
                    new IocParameter("ParameterProperty", "ParameterProperty", IocParameterType.Property)
                };
                iocContainer.Bind(typeof(ParameterClass), typeof(ParameterClass), DependencyLifecycle.SingleInstance);
                var parameterClass = iocContainer.Get<ParameterClass>(parameters: parameters);
                parameterClass.ParameterConstructor.ShouldEqual(parameters[0].Value);
                parameterClass.ParameterProperty.ShouldEqual(parameters[1].Value);
            }
        }

        [TestMethod]
        public virtual void ShouldResolveClassWithPredefinedParameters()
        {
            using (IIocContainer iocContainer = GetIocContainer())
            {
                var parameters = new IIocParameter[]
                {
                    new IocParameter("parameterConstructor", "parameterConstructor", IocParameterType.Constructor),
                    new IocParameter("ParameterProperty", "ParameterProperty", IocParameterType.Property)
                };
                iocContainer.Bind(typeof(ParameterClass), typeof(ParameterClass), DependencyLifecycle.SingleInstance, parameters: parameters);
                var parameterClass = iocContainer.Get<ParameterClass>();
                parameterClass.ParameterConstructor.ShouldEqual(parameters[0].Value);
                parameterClass.ParameterProperty.ShouldEqual(parameters[1].Value);
            }
        }

        #endregion
    }

    public class ParameterClass
    {
        #region Constructors

        public ParameterClass(string parameterConstructor)
        {
            ParameterConstructor = parameterConstructor;
        }

        #endregion

        #region Properties

        public string ParameterConstructor { get; set; }

#if !NETFX_CORE
        [Ninject.Inject]
#endif
        public string ParameterProperty { get; set; }

        #endregion
    }

    public interface ISimple
    {
    }

    public class Simple : ISimple
    {
        public Guid Guid = Guid.NewGuid();
    }

    public class ComplexDependency
    {
        public ISimple Simple { get; }
        public IEnumerable<ISimple> Simples { get; }

        public ComplexDependency(ISimple simple, IEnumerable<ISimple> simples)
        {
            Simple = simple;
            Simples = simples;
        }
    }
}
