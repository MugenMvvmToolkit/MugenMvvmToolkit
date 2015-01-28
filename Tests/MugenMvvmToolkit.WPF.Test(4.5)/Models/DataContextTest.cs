using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Should;

namespace MugenMvvmToolkit.Test.Models
{
    [TestClass]
    public class DataContextTest : TestBase
    {
        #region Methods

        [TestMethod]
        public void AddGetTryGetTest()
        {
            const string st = "test";
            IDataContext context = Create();
            context.Add(InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Mixed);
            context.Count.ShouldEqual(1);

            context.Add(InitializationConstants.ViewModelBindingName, st);
            context.Count.ShouldEqual(2);

            IocContainerCreationMode data;
            context.TryGetData(InitializationConstants.IocContainerCreationMode, out data).ShouldBeTrue();
            data.ShouldEqual(IocContainerCreationMode.Mixed);
            context.GetData(InitializationConstants.ViewModelBindingName).ShouldEqual(st);
        }

        [TestMethod]
        public void ContainsRemoveTest()
        {
            IDataContext context = Create();
            context.Add(InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Mixed);
            context.Contains(InitializationConstants.IocContainerCreationMode).ShouldBeTrue();

            context.Remove(InitializationConstants.IocContainerCreationMode).ShouldBeTrue();
            context.Contains(InitializationConstants.IocContainerCreationMode).ShouldBeFalse();
            context.Remove(InitializationConstants.IocContainerCreationMode).ShouldBeFalse();
        }

        [TestMethod]
        public void EnumerableTest()
        {
            const string st = "test";
            IDataContext context = Create();
            context.Add(InitializationConstants.IocContainerCreationMode, IocContainerCreationMode.Mixed);
            context.Count.ShouldEqual(1);

            context.Add(InitializationConstants.ViewModelBindingName, st);
            context.Count.ShouldEqual(2);

            var dictionary = context.ToList().ToDictionary(constantValue => constantValue.DataConstant, value => value.Value);
            dictionary.Count.ShouldEqual(2);
            dictionary[InitializationConstants.IocContainerCreationMode].ShouldEqual(IocContainerCreationMode.Mixed);
            dictionary[InitializationConstants.ViewModelBindingName].ShouldEqual(st);
        }

        [TestMethod]
        public void AddNotNullValueShouldThrowException()
        {
            var dataConstant = new DataConstant<object>("id", true);
            var dataContext = Create();
            ShouldThrow(() => dataContext.Add(dataConstant, null));
        }

        protected virtual IDataContext Create()
        {
            return new DataContext();
        }

        #endregion
    }

    [TestClass]
    public class DataContextSerializationTest : SerializationTestBase<DataContext>
    {
        #region Overrides of SerializationTestBase<DataContext>

        [Ignore]
        public override void TestXmlSerialization()
        {
            base.TestXmlSerialization();
        }

        protected override DataContext GetObject()
        {
            return new DataContext
            {
                {InitializationConstants.ObservationMode, ObservationMode.Both},
                {InitializationConstants.ViewName, "Test"}
            };
        }

        protected override void AssertObject(DataContext deserializedObj)
        {
            deserializedObj.Count.ShouldEqual(2);
            deserializedObj.GetDataTest(InitializationConstants.ObservationMode).ShouldEqual(ObservationMode.Both);
            deserializedObj.GetData(InitializationConstants.ViewName).ShouldEqual("Test");
        }

        protected override IEnumerable<Type> GetKnownTypes()
        {
            return new Type[] { typeof(DataConstant), typeof(string), typeof(ObservationMode) };
        }

        #endregion
    }
}